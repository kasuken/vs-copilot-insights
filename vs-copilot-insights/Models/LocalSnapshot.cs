using System.Text.Json.Serialization;

namespace vs_copilot_insights.Models;

/// <summary>
/// A locally stored point-in-time record of AI credit (premium_interactions) usage,
/// used for trend charts, delta comparisons, prediction, and burn-rate analysis.
/// </summary>
internal sealed class LocalSnapshot
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("premium_remaining")]
    public double PremiumRemaining { get; set; }

    [JsonPropertyName("premium_entitlement")]
    public double PremiumEntitlement { get; set; }
}
