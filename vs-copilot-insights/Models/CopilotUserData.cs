using System.Text.Json.Serialization;

namespace vs_copilot_insights.Models;

internal sealed class Organization
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

internal sealed class QuotaSnapshot
{
    [JsonPropertyName("quota_id")]
    public string QuotaId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp_utc")]
    public string TimestampUtc { get; set; } = string.Empty;

    [JsonPropertyName("entitlement")]
    public double Entitlement { get; set; }

    [JsonPropertyName("quota_remaining")]
    public double QuotaRemaining { get; set; }

    [JsonPropertyName("remaining")]
    public double Remaining { get; set; }

    [JsonPropertyName("percent_remaining")]
    public double PercentRemaining { get; set; }

    [JsonPropertyName("unlimited")]
    public bool Unlimited { get; set; }

    [JsonPropertyName("overage_permitted")]
    public bool OveragePermitted { get; set; }

    [JsonPropertyName("overage_count")]
    public double OverageCount { get; set; }
}

internal sealed class CopilotUserData
{
    [JsonPropertyName("copilot_plan")]
    public string CopilotPlan { get; set; } = string.Empty;

    [JsonPropertyName("chat_enabled")]
    public bool ChatEnabled { get; set; }

    [JsonPropertyName("access_type_sku")]
    public string AccessTypeSku { get; set; } = string.Empty;

    [JsonPropertyName("assigned_date")]
    public string AssignedDate { get; set; } = string.Empty;

    [JsonPropertyName("organization_list")]
    public List<Organization>? OrganizationList { get; set; }

    [JsonPropertyName("quota_snapshots")]
    public Dictionary<string, QuotaSnapshot>? QuotaSnapshots { get; set; }

    [JsonPropertyName("quota_reset_date_utc")]
    public string QuotaResetDateUtc { get; set; } = string.Empty;

    [JsonPropertyName("quota_reset_date")]
    public string QuotaResetDate { get; set; } = string.Empty;

    [JsonPropertyName("tracking_id")]
    public string? TrackingId { get; set; }
}
