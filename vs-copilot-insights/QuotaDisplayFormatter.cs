using System.Globalization;

namespace vs_copilot_insights;

/// <summary>
/// Pure display-formatting helpers extracted from <see cref="CopilotInsightsViewModel"/>
/// so they can be tested and reused independently.
/// </summary>
internal static class QuotaDisplayFormatter
{
    /// <summary>The quota ID string for the premium AI-credits quota.</summary>
    internal const string PremiumQuotaId = "premium_interactions";

    public static string Format(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    public static string FormatDelta(double? delta)
    {
        if (delta is not { } value)
        {
            return string.Empty;
        }

        if (value == 0)
        {
            return "No change";
        }

        return value > 0
            ? $"\u25B2 +{Format(value)} credits"
            : $"\u25BC {Format(value)} credits";
    }

    public static string FormatSnapshotTime(string timestamp)
    {
        if (DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset parsed))
        {
            return parsed.ToLocalTime().ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture);
        }

        return timestamp;
    }

    public static string FormatResetInShort(TimeSpan diff)
    {
        int days = Math.Max(0, (int)diff.TotalDays);
        int hours = Math.Max(0, diff.Hours);
        return $"{days}d {hours}h";
    }

    public static string FormatQuotaName(string quotaId) =>
        quotaId switch
        {
            PremiumQuotaId => "AI Credits",
            "completions" => "Suggestions",
            _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(quotaId.Replace('_', ' ')),
        };

    public static string NormalizePlan(string plan)
    {
        string trimmed = plan.Trim();
        return trimmed.Length > 0
            ? char.ToUpper(trimmed[0]) + trimmed[1..]
            : trimmed;
    }

    public static (string Emoji, string Label) GetStatusBadge(double percentRemaining) =>
        percentRemaining switch
        {
            <= 0 => ("\U0001F6AB", "Over Quota"),
            > 50 => ("\U0001F7E2", "Healthy"),
            >= 20 => ("\U0001F7E1", "Watch"),
            _ => ("\U0001F534", "Risk"),
        };

    public static string GetMood(double percentRemaining) =>
        percentRemaining switch
        {
            <= 0 => "\U0001F480 Over quota",
            > 75 => "\U0001F60C Plenty of quota left",
            > 40 => "\U0001F642 You\u2019re fine",
            > 15 => "\U0001F62C Getting tight",
            _ => "\U0001F631 Danger zone",
        };
}
