using System.Globalization;
using vs_copilot_insights.Models;

namespace vs_copilot_insights.Services;

internal readonly record struct SnapshotComparison(double? SinceLastRefresh, double? SinceYesterday);

internal sealed class WeightedPrediction
{
    public int PredictedDailyUsage { get; init; }
    public string Confidence { get; init; } = "low";
    public string ConfidenceReason { get; init; } = string.Empty;
    public int? DaysUntilExhaustion { get; init; }
    public bool WillExhaustBeforeReset { get; init; }
    public int DataPoints { get; init; }
}

internal sealed class TrendPrediction
{
    public int RecentBurnRate { get; init; }
    public int OverallBurnRate { get; init; }
    public string Trend { get; init; } = "stable";
    public string TrendIndicator { get; init; } = string.Empty;
    public int DataPoints { get; init; }
}

/// <summary>
/// Local-history analytics ported from the VS Code extension: snapshot recording,
/// delta comparisons, weighted usage prediction, and burn-rate / trend analysis.
/// All AI credit values refer to the <c>premium_interactions</c> quota.
/// </summary>
internal static class UsageAnalytics
{
    public const int MaxSnapshots = 90;

    /// <summary>
    /// Records a new snapshot at the front of <paramref name="history"/>, mirroring the
    /// VS Code <c>_addSnapshot</c> behavior (skip invalid entitlements, skip duplicates,
    /// cap to <see cref="MaxSnapshots"/>). Returns true if a snapshot was added.
    /// </summary>
    public static bool AddSnapshot(List<LocalSnapshot> history, double premiumRemaining, double premiumEntitlement)
    {
        if (premiumEntitlement <= 0)
        {
            return false;
        }

        if (history.Count > 0 && history[0].PremiumRemaining == premiumRemaining)
        {
            return false;
        }

        history.Insert(0, new LocalSnapshot
        {
            Timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            PremiumRemaining = premiumRemaining,
            PremiumEntitlement = premiumEntitlement,
        });

        if (history.Count > MaxSnapshots)
        {
            history.RemoveRange(MaxSnapshots, history.Count - MaxSnapshots);
        }

        return true;
    }

    public static SnapshotComparison GetComparisons(IReadOnlyList<LocalSnapshot> history)
    {
        if (history.Count < 2)
        {
            return new SnapshotComparison(null, null);
        }

        LocalSnapshot current = history[0];
        LocalSnapshot previousRefresh = history[1];

        double sinceLastRefresh = current.PremiumRemaining - previousRefresh.PremiumRemaining;

        double now = ParseTimestamp(current.Timestamp);
        double oneDayAgo = now - 24 * 60 * 60 * 1000;

        LocalSnapshot? closestYesterday = null;
        double closestTimeDiff = double.PositiveInfinity;

        foreach (LocalSnapshot snapshot in history)
        {
            double snapshotTime = ParseTimestamp(snapshot.Timestamp);
            double timeDiff = Math.Abs(snapshotTime - oneDayAgo);

            if (snapshotTime <= now - 12 * 60 * 60 * 1000 && timeDiff < closestTimeDiff)
            {
                closestTimeDiff = timeDiff;
                closestYesterday = snapshot;
            }
        }

        double? sinceYesterday = closestYesterday is { } y
            ? current.PremiumRemaining - y.PremiumRemaining
            : null;

        return new SnapshotComparison(sinceLastRefresh, sinceYesterday);
    }

    public static WeightedPrediction? GetWeightedPrediction(
        IReadOnlyList<LocalSnapshot> history,
        double? effectiveRemaining,
        DateTime? resetDateUtc)
    {
        List<double> usageData = BuildDailyUsageSeries(history);
        if (usageData.Count == 0)
        {
            return null;
        }

        double predictedDailyUsage = usageData.Sum() / usageData.Count;

        int totalDataPoints = usageData.Count;
        string confidence;
        string confidenceReason;
        if (totalDataPoints >= 7)
        {
            confidence = "high";
            confidenceReason = $"Based on {totalDataPoints} data points from local history";
        }
        else if (totalDataPoints >= 3)
        {
            confidence = "medium";
            confidenceReason = $"Based on {totalDataPoints} data points from local history";
        }
        else
        {
            confidence = "low";
            confidenceReason = $"Limited data: only {totalDataPoints} data point{(totalDataPoints > 1 ? "s" : "")} available";
        }

        int? daysUntilExhaustion = null;
        bool willExhaustBeforeReset = false;

        if (effectiveRemaining is { } remaining && predictedDailyUsage > 0)
        {
            daysUntilExhaustion = (int)Math.Floor(remaining / predictedDailyUsage);

            if (resetDateUtc is { } reset)
            {
                double daysUntilReset = (reset - DateTime.UtcNow).TotalDays;
                willExhaustBeforeReset = daysUntilExhaustion < daysUntilReset;
            }
        }

        return new WeightedPrediction
        {
            PredictedDailyUsage = RoundHalfUp(predictedDailyUsage),
            Confidence = confidence,
            ConfidenceReason = confidenceReason,
            DaysUntilExhaustion = daysUntilExhaustion,
            WillExhaustBeforeReset = willExhaustBeforeReset,
            DataPoints = totalDataPoints,
        };
    }

    public static TrendPrediction? GetTrendPrediction(IReadOnlyList<LocalSnapshot> history)
    {
        if (history.Count < 3)
        {
            return null;
        }

        List<double> usageData = BuildDailyUsageSeries(history);
        if (usageData.Count < 2)
        {
            return null;
        }

        double overallBurnRate = usageData.Sum() / usageData.Count;

        int recentCount = Math.Max(2, (int)Math.Ceiling(usageData.Count / 2.0));
        List<double> recentData = usageData.Take(recentCount).ToList();
        double recentBurnRate = recentData.Sum() / recentData.Count;

        double difference = recentBurnRate - overallBurnRate;
        double percentDiff = overallBurnRate > 0 ? difference / overallBurnRate * 100 : 0;

        string trend;
        string trendIndicator;
        if (Math.Abs(percentDiff) < 10)
        {
            trend = "stable";
            trendIndicator = "No significant change";
        }
        else if (difference > 0)
        {
            trend = "accelerating";
            trendIndicator = $"+{RoundHalfUp(Math.Abs(percentDiff))}% vs average";
        }
        else
        {
            trend = "slowing";
            trendIndicator = $"-{RoundHalfUp(Math.Abs(percentDiff))}% vs average";
        }

        return new TrendPrediction
        {
            RecentBurnRate = RoundHalfUp(recentBurnRate),
            OverallBurnRate = RoundHalfUp(overallBurnRate),
            Trend = trend,
            TrendIndicator = trendIndicator,
            DataPoints = usageData.Count,
        };
    }

    /// <summary>
    /// Builds the list of normalized daily-usage values between consecutive snapshots,
    /// considering only gaps of 1-72 hours with positive consumption.
    /// </summary>
    private static List<double> BuildDailyUsageSeries(IReadOnlyList<LocalSnapshot> history)
    {
        var usageData = new List<double>();

        for (int i = 0; i < history.Count - 1; i++)
        {
            LocalSnapshot current = history[i];
            LocalSnapshot previous = history[i + 1];

            double hoursDiff = (ParseTimestamp(current.Timestamp) - ParseTimestamp(previous.Timestamp)) / (1000 * 60 * 60);

            if (hoursDiff >= 1 && hoursDiff <= 72)
            {
                double usage = previous.PremiumRemaining - current.PremiumRemaining;
                if (usage > 0)
                {
                    usageData.Add(usage / hoursDiff * 24);
                }
            }
        }

        return usageData;
    }

    /// <summary>Returns the timestamp as milliseconds since the Unix epoch (UTC).</summary>
    private static double ParseTimestamp(string timestamp)
    {
        if (DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset parsed))
        {
            return parsed.ToUnixTimeMilliseconds();
        }

        return 0;
    }

    /// <summary>Rounds half away from zero to match JavaScript's <c>Math.round</c> for non-negative values.</summary>
    private static int RoundHalfUp(double value) => (int)Math.Floor(value + 0.5);
}
