using System.Globalization;
using System.Text;
using System.Text.Json;
using vs_copilot_insights.Models;

namespace vs_copilot_insights.Services;

/// <summary>
/// Builds the shareable Markdown summary and the raw JSON payload exported from the
/// Insights view. Ported from the VS Code <c>_generateMarkdownSummary</c> implementation.
/// </summary>
internal static class MarkdownExporter
{
    private const double CreditCostUsd = 0.01;

    public static string BuildMarkdown(CopilotUserData data, double customCreditLimit)
    {
        List<QuotaSnapshot> quotas = data.QuotaSnapshots is { } map ? map.Values.ToList() : [];

        QuotaSnapshot? latest = quotas.Count > 0 ? quotas[0] : null;
        string asOfTime = !string.IsNullOrWhiteSpace(latest?.TimestampUtc)
            ? latest!.TimestampUtc
            : DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        (int days, int hours, double totalDays) reset = CalculateDaysUntilReset(data.QuotaResetDateUtc, asOfTime);

        var sb = new StringBuilder();
        sb.Append("# GitHub Copilot Insights\n\n");
        sb.Append($"**Generated:** {DateTime.Now.ToString("G", CultureInfo.CurrentCulture)}\n\n");

        sb.Append("## Plan Details\n\n");
        sb.Append($"- **Plan:** {Or(data.CopilotPlan, "Unknown")}\n");
        sb.Append($"- **Chat:** {Enabled(data.ChatEnabled)}\n");
        sb.Append($"- **CLI:** {Enabled(data.CliEnabled)}\n");
        sb.Append($"- **MCP:** {Enabled(data.IsMcpEnabled)}\n");
        sb.Append($"- **Preview Features:** {Enabled(data.EditorPreviewFeaturesEnabled)}\n");
        sb.Append($"- **Access/SKU:** {Or(data.AccessTypeSku, "Unknown")}\n");
        sb.Append($"- **Assigned:** {FormatDate(data.AssignedDate)}\n\n");

        if (quotas.Count > 0)
        {
            sb.Append("## Quotas\n\n");
            foreach (QuotaSnapshot quota in quotas)
            {
                sb.Append($"### {FormatQuotaName(quota.QuotaId)}\n\n");

                if (quota.Unlimited)
                {
                    sb.Append("- **Status:** Unlimited \u221E\n\n");
                    continue;
                }

                QuotaSnapshot eq = quota.QuotaId == "premium_interactions"
                    ? GetEffectiveQuota(quota, customCreditLimit)
                    : quota;

                double used = eq.Entitlement - eq.QuotaRemaining;
                bool isOverQuota = eq.Remaining < 0;
                double percentRemaining = eq.Entitlement > 0
                    ? Math.Round(eq.QuotaRemaining / eq.Entitlement * 100, 1)
                    : 0;
                double overageAmount = isOverQuota ? Math.Round(Math.Abs(eq.Remaining), 1) : 0;
                (string emoji, string label) = GetStatusBadge(percentRemaining);

                if (isOverQuota)
                {
                    sb.Append($"- **Status:** {emoji} {label} (exceeded by {Num(overageAmount)})\n");
                    sb.Append($"- **Over by:** {Num(overageAmount)}\n");
                }
                else
                {
                    sb.Append($"- **Status:** {emoji} {label} ({Num(percentRemaining)}% remaining)\n");
                    sb.Append($"- **Remaining:** {Num(eq.Remaining)}\n");
                }

                sb.Append($"- **Used:** {Num(used)}\n");
                sb.Append($"- **Total:** {Num(eq.Entitlement)}\n");

                if (reset.totalDays > 0)
                {
                    if (!isOverQuota)
                    {
                        long allowedPerDay = (long)Math.Floor(eq.Remaining / reset.totalDays);
                        sb.Append($"- **To last until reset:** \u2264 {allowedPerDay}/day\n");
                    }

                    sb.Append($"- **Reset in:** {reset.days}d {reset.hours}h\n");
                    sb.Append($"- **Reset Date:** {FormatDate(data.QuotaResetDateUtc)}\n");
                }

                if (quota.OveragePermitted)
                {
                    sb.Append("- **Overage:** Permitted");
                    if (isOverQuota)
                    {
                        double billableOverage = Math.Max(0, used - quota.Entitlement);
                        sb.Append($" ({Num(billableOverage)} billable, est. cost: ${billableOverage * CreditCostUsd:F2})");
                    }
                    else if (quota.OverageCount > 0)
                    {
                        sb.Append($" ({Num(quota.OverageCount)} used)");
                    }

                    sb.Append('\n');
                }

                sb.Append('\n');
            }
        }

        if (data.OrganizationList is { Count: > 0 } orgs)
        {
            sb.Append("## Organizations with Copilot Access\n\n");
            foreach (Organization org in orgs)
            {
                sb.Append($"- **{Or(org.Name, org.Login)}** (@{org.Login})\n");
            }

            sb.Append('\n');
        }

        sb.Append("---\n");
        sb.Append("*Data fetched from GitHub Copilot API*\n");

        return sb.ToString();
    }

    public static string BuildJson(CopilotUserData data)
    {
        if (!string.IsNullOrWhiteSpace(data.RawJson))
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(data.RawJson);
                return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                // Fall through to serializing the typed object.
            }
        }

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    private static QuotaSnapshot GetEffectiveQuota(QuotaSnapshot quota, double customLimit)
    {
        double planEntitlement = quota.Entitlement;
        double used = Math.Max(0, planEntitlement - quota.QuotaRemaining);

        if (customLimit > planEntitlement)
        {
            double effectiveRemaining = customLimit - used;
            return new QuotaSnapshot
            {
                QuotaId = quota.QuotaId,
                TimestampUtc = quota.TimestampUtc,
                Entitlement = customLimit,
                QuotaRemaining = effectiveRemaining,
                Remaining = effectiveRemaining,
                PercentRemaining = quota.PercentRemaining,
                Unlimited = quota.Unlimited,
                OveragePermitted = quota.OveragePermitted,
                OverageCount = quota.OverageCount,
            };
        }

        return quota;
    }

    private static (string Emoji, string Label) GetStatusBadge(double percentRemaining)
    {
        return percentRemaining switch
        {
            <= 0 => ("\U0001F6AB", "Over Quota"),
            > 50 => ("\U0001F7E2", "Healthy"),
            >= 20 => ("\U0001F7E1", "Watch"),
            _ => ("\U0001F534", "Risk"),
        };
    }

    private static string FormatQuotaName(string quotaId)
    {
        return quotaId switch
        {
            "premium_interactions" => "AI Credits",
            "completions" => "Suggestions",
            _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(quotaId.Replace('_', ' ')),
        };
    }

    private static (int Days, int Hours, double TotalDays) CalculateDaysUntilReset(string resetDate, string asOfTime)
    {
        if (!TryParseDate(resetDate, out DateTimeOffset reset) || !TryParseDate(asOfTime, out DateTimeOffset asOf))
        {
            return (0, 0, 0);
        }

        double diffDays = (reset - asOf).TotalDays;
        int days = (int)Math.Floor(diffDays);
        int hours = (int)Math.Floor((diffDays - days) * 24);
        return (days, hours, diffDays);
    }

    private static string FormatDate(string dateStr)
    {
        return TryParseDate(dateStr, out DateTimeOffset date)
            ? date.ToLocalTime().ToString("MMM d, yyyy", CultureInfo.InvariantCulture)
            : dateStr;
    }

    private static bool TryParseDate(string value, out DateTimeOffset result)
    {
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
    }

    private static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Enabled(bool value) => value ? "Enabled" : "Disabled";

    private static string Or(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;
}
