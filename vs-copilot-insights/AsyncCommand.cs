using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.UI;

namespace vs_copilot_insights;

internal sealed class AsyncCommand(Func<object?, CancellationToken, Task> executeAsync) : IAsyncCommand
{
    private volatile int _isBusy;

    public bool CanExecute => _isBusy == 0;

    public async Task ExecuteAsync(object? parameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _isBusy, 1, 0) != 0)
        {
            return;
        }

        try
        {
            await executeAsync(parameter, cancellationToken);
        }
        finally
        {
            Interlocked.Exchange(ref _isBusy, 0);
        }
    }
}
