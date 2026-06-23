using System.Text.Json.Serialization;

namespace vs_copilot_insights.Models;

/// <summary>
/// User-configurable settings, persisted locally. Mirrors the relevant
/// VS Code <c>copilotInsights.*</c> configuration entries.
/// </summary>
internal sealed class InsightsSettings
{
    public const int DefaultPollingIntervalSeconds = 60;

    /// <summary>
    /// Custom AI credit limit above the plan entitlement. 0 uses the plan default.
    /// </summary>
    [JsonPropertyName("customCreditLimit")]
    public double CustomCreditLimit { get; set; }

    /// <summary>
    /// Automatic refresh interval in seconds. 0 disables background polling.
    /// </summary>
    [JsonPropertyName("pollingIntervalSeconds")]
    public int PollingIntervalSeconds { get; set; } = DefaultPollingIntervalSeconds;
}
