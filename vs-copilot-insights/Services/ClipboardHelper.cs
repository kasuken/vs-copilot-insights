using System.Runtime.InteropServices;
using System.Text;

namespace vs_copilot_insights.Services;

/// <summary>
/// Places Unicode text on the Windows clipboard from the out-of-process extension.
/// VisualStudio.Extensibility exposes no clipboard API, so this uses the Win32 clipboard
/// functions on a dedicated STA thread (required by OLE clipboard semantics).
/// </summary>
internal static class ClipboardHelper
{
    private const uint CfUnicodeText = 13;
    private const uint GmemMovable = 0x0002;

    public static bool SetText(string text)
    {
        bool success = false;

        var thread = new Thread(() => success = TrySetText(text))
        {
            IsBackground = true,
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        bool finished = thread.Join(TimeSpan.FromSeconds(5));

        return finished && success;
    }

    private static bool TrySetText(string text)
    {
        // The clipboard can be transiently locked by another process; retry briefly.
        bool opened = false;
        for (int attempt = 0; attempt < 10 && !opened; attempt++)
        {
            opened = OpenClipboard(IntPtr.Zero);
            if (!opened)
            {
                Thread.Sleep(20);
            }
        }

        if (!opened)
        {
            return false;
        }

        IntPtr hGlobal = IntPtr.Zero;
        try
        {
            EmptyClipboard();

            byte[] bytes = Encoding.Unicode.GetBytes(text + "\0");
            hGlobal = GlobalAlloc(GmemMovable, (UIntPtr)bytes.Length);
            if (hGlobal == IntPtr.Zero)
            {
                return false;
            }

            IntPtr target = GlobalLock(hGlobal);
            if (target == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                Marshal.Copy(bytes, 0, target, bytes.Length);
            }
            finally
            {
                GlobalUnlock(hGlobal);
            }

            if (SetClipboardData(CfUnicodeText, hGlobal) == IntPtr.Zero)
            {
                return false;
            }

            // On success the system owns the memory; prevent the finally from freeing it.
            hGlobal = IntPtr.Zero;
            return true;
        }
        finally
        {
            if (hGlobal != IntPtr.Zero)
            {
                GlobalFree(hGlobal);
            }

            CloseClipboard();
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);
}
