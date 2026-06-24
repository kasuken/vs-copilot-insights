using System.Text.Json;
using vs_copilot_insights.Models;

namespace vs_copilot_insights.Services;

/// <summary>
/// Persists snapshot history, user settings, and small pieces of state to JSON files
/// under <c>%LOCALAPPDATA%\vs-copilot-insights</c>. This is the out-of-process
/// equivalent of the VS Code extension's <c>context.globalState</c>.
/// </summary>
internal sealed class LocalStorageService
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    private readonly object _gate = new();
    private readonly string _directory;
    private readonly string _snapshotsPath;
    private readonly string _settingsPath;
    private readonly string _statePath;

    public LocalStorageService()
    {
        _directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "vs-copilot-insights");
        _snapshotsPath = Path.Combine(_directory, "snapshots.json");
        _settingsPath = Path.Combine(_directory, "settings.json");
        _statePath = Path.Combine(_directory, "state.json");
    }

    public List<LocalSnapshot> LoadSnapshots()
    {
        lock (_gate)
        {
            return ReadJson<List<LocalSnapshot>>(_snapshotsPath) ?? [];
        }
    }

    public void SaveSnapshots(IReadOnlyList<LocalSnapshot> snapshots)
    {
        lock (_gate)
        {
            WriteJson(_snapshotsPath, snapshots);
        }
    }

    public InsightsSettings LoadSettings()
    {
        lock (_gate)
        {
            return ReadJson<InsightsSettings>(_settingsPath) ?? new InsightsSettings();
        }
    }

    public void SaveSettings(InsightsSettings settings)
    {
        lock (_gate)
        {
            WriteJson(_settingsPath, settings);
        }
    }

    public string? GetLastNotifiedReset()
    {
        lock (_gate)
        {
            return ReadJson<StateData>(_statePath)?.LastNotifiedReset;
        }
    }

    public void SetLastNotifiedReset(string? resetDate)
    {
        lock (_gate)
        {
            WriteJson(_statePath, new StateData { LastNotifiedReset = resetDate });
        }
    }

    private T? ReadJson<T>(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return default;
            }

            string json = File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    private void WriteJson<T>(string path, T value)
    {
        try
        {
            Directory.CreateDirectory(_directory);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(value, s_jsonOptions));
            File.Move(tmp, path, overwrite: true);
        }
        catch
        {
            // Storage is best-effort; failures must never break the UI.
        }
    }

    private sealed class StateData
    {
        public string? LastNotifiedReset { get; set; }
    }
}
