using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.UI;
using vs_copilot_insights.Models;
using vs_copilot_insights.Services;

namespace vs_copilot_insights;

[DataContract]
internal sealed class QuotaCardViewModel : NotifyPropertyChangedBase
{
    private string _name = string.Empty;
    private string _statusEmoji = string.Empty;
    private string _statusLabel = string.Empty;
    private double _progressValue;
    private string _usageDisplay = string.Empty;
    private string _percentDisplay = string.Empty;
    private bool _isOverQuota;
    private bool _isLimited;
    private string _overageDisplay = string.Empty;
    private bool _showPremiumStats;
    private string _premiumAllowancePerDay = string.Empty;
    private string _premiumResetIn = string.Empty;
    private string _premiumResetDate = string.Empty;
    private string _premiumWeeklyAverage = string.Empty;
    private string _premiumWorkdayAverage = string.Empty;
    private string _premiumWorkhourAverage = string.Empty;
    private string _premiumEfficientCapacity = string.Empty;
    private string _premiumStandardCapacity = string.Empty;
    private string _premiumAdvancedCapacity = string.Empty;
    private string _premiumOveragePolicy = string.Empty;
    private string _premiumMood = string.Empty;
    private string _premiumRemainingPercent = string.Empty;
    private string _premiumRemaining = string.Empty;
    private string _premiumUsed = string.Empty;
    private string _premiumTotal = string.Empty;

    [DataMember] public string Name { get => _name; set => SetField(ref _name, value); }
    [DataMember] public string StatusEmoji { get => _statusEmoji; set => SetField(ref _statusEmoji, value); }
    [DataMember] public string StatusLabel { get => _statusLabel; set => SetField(ref _statusLabel, value); }
    [DataMember] public double ProgressValue { get => _progressValue; set => SetField(ref _progressValue, value); }
    [DataMember] public string UsageDisplay { get => _usageDisplay; set => SetField(ref _usageDisplay, value); }
    [DataMember] public string PercentDisplay { get => _percentDisplay; set => SetField(ref _percentDisplay, value); }
    [DataMember] public bool IsOverQuota { get => _isOverQuota; set => SetField(ref _isOverQuota, value); }
    [DataMember] public bool IsLimited { get => _isLimited; set => SetField(ref _isLimited, value); }
    [DataMember] public string OverageDisplay { get => _overageDisplay; set => SetField(ref _overageDisplay, value); }
    [DataMember] public bool ShowPremiumStats { get => _showPremiumStats; set => SetField(ref _showPremiumStats, value); }
    [DataMember] public string PremiumAllowancePerDay { get => _premiumAllowancePerDay; set => SetField(ref _premiumAllowancePerDay, value); }
    [DataMember] public string PremiumResetIn { get => _premiumResetIn; set => SetField(ref _premiumResetIn, value); }
    [DataMember] public string PremiumResetDate { get => _premiumResetDate; set => SetField(ref _premiumResetDate, value); }
    [DataMember] public string PremiumWeeklyAverage { get => _premiumWeeklyAverage; set => SetField(ref _premiumWeeklyAverage, value); }
    [DataMember] public string PremiumWorkdayAverage { get => _premiumWorkdayAverage; set => SetField(ref _premiumWorkdayAverage, value); }
    [DataMember] public string PremiumWorkhourAverage { get => _premiumWorkhourAverage; set => SetField(ref _premiumWorkhourAverage, value); }
    [DataMember] public string PremiumEfficientCapacity { get => _premiumEfficientCapacity; set => SetField(ref _premiumEfficientCapacity, value); }
    [DataMember] public string PremiumStandardCapacity { get => _premiumStandardCapacity; set => SetField(ref _premiumStandardCapacity, value); }
    [DataMember] public string PremiumAdvancedCapacity { get => _premiumAdvancedCapacity; set => SetField(ref _premiumAdvancedCapacity, value); }
    [DataMember] public string PremiumOveragePolicy { get => _premiumOveragePolicy; set => SetField(ref _premiumOveragePolicy, value); }
    [DataMember] public string PremiumMood { get => _premiumMood; set => SetField(ref _premiumMood, value); }
    [DataMember] public string PremiumRemainingPercent { get => _premiumRemainingPercent; set => SetField(ref _premiumRemainingPercent, value); }
    [DataMember] public string PremiumRemaining { get => _premiumRemaining; set => SetField(ref _premiumRemaining, value); }
    [DataMember] public string PremiumUsed { get => _premiumUsed; set => SetField(ref _premiumUsed, value); }
    [DataMember] public string PremiumTotal { get => _premiumTotal; set => SetField(ref _premiumTotal, value); }

}

[DataContract]
internal sealed class ChartBarViewModel : NotifyPropertyChangedBase
{
    private double _height;
    private string _tooltip = string.Empty;

    [DataMember] public double Height { get => _height; set => SetField(ref _height, value); }
    [DataMember] public string Tooltip { get => _tooltip; set => SetField(ref _tooltip, value); }

}

[DataContract]
internal sealed class CopilotInsightsViewModel : NotifyPropertyChangedBase, IDisposable
{
    private readonly GitHubCopilotService _service;
    private readonly VisualStudioExtensibility _extensibility;
    private readonly LocalStorageService _storage;

    // Under GitHub's AI Credits billing model, 1 AI credit costs $0.01 USD.
    private const double CreditCostUsd = 0.01;
    private const double PremiumUsageAlertThreshold = 85;
    private const string PremiumQuotaId = QuotaDisplayFormatter.PremiumQuotaId;
    private const int WorkingHoursPerDay = 8;
    private const double EfficientModelCostMultiplier = 0.33;
    private const double AdvancedModelCostMultiplier = 3.0;
    private const int DaysPerMonth = 30;

    private readonly record struct PacingBudget(
        long Daily,
        long Weekly,
        long PerWorkday,
        long PerHour,
        long BudgetEfficient,
        long BudgetStandard,
        long BudgetAdvanced);

    private readonly List<LocalSnapshot> _snapshotHistory;
    private InsightsSettings _settings;
    private CopilotUserData? _latestData;
    private CancellationTokenSource? _pollingCts;
    private readonly CancellationTokenSource _initialLoadCts = new();
    private readonly object _pollingGate = new();
    private readonly SemaphoreSlim _refreshGate = new(1, 1);

    private bool _isLoading = true;
    private bool _hasError;
    private bool _hasData;
    private string _errorMessage = string.Empty;
    private string _planValue = string.Empty;
    private string _chatValue = string.Empty;
    private string _skuValue = string.Empty;
    private string _orgValue = string.Empty;
    private string _resetCountdown = string.Empty;
    private string _resetDateDisplay = string.Empty;
    private string _dailyValue = string.Empty;
    private string _weeklyValue = string.Empty;
    private string _workdayValue = string.Empty;
    private bool _hasPacing;

    [DataMember] public bool IsLoading { get => _isLoading; set => SetField(ref _isLoading, value); }
    [DataMember] public bool HasError { get => _hasError; set => SetField(ref _hasError, value); }
    [DataMember] public bool HasData { get => _hasData; set => SetField(ref _hasData, value); }
    [DataMember] public string ErrorMessage { get => _errorMessage; set => SetField(ref _errorMessage, value); }
    [DataMember] public string PlanValue { get => _planValue; set => SetField(ref _planValue, value); }
    [DataMember] public string ChatValue { get => _chatValue; set => SetField(ref _chatValue, value); }
    [DataMember] public string SkuValue { get => _skuValue; set => SetField(ref _skuValue, value); }
    [DataMember] public string OrgValue { get => _orgValue; set => SetField(ref _orgValue, value); }
    [DataMember] public string ResetCountdown { get => _resetCountdown; set => SetField(ref _resetCountdown, value); }
    [DataMember] public string ResetDateDisplay { get => _resetDateDisplay; set => SetField(ref _resetDateDisplay, value); }
    [DataMember] public string DailyValue { get => _dailyValue; set => SetField(ref _dailyValue, value); }
    [DataMember] public string WeeklyValue { get => _weeklyValue; set => SetField(ref _weeklyValue, value); }
    [DataMember] public string WorkdayValue { get => _workdayValue; set => SetField(ref _workdayValue, value); }
    [DataMember] public bool HasPacing { get => _hasPacing; set => SetField(ref _hasPacing, value); }

    // Usage trend: snapshot chart + delta comparisons
    private bool _hasUsageTrend;
    private bool _hasChart;
    private bool _hasComparisons;
    private bool _hasSinceLastRefresh;
    private bool _hasSinceYesterday;
    private string _sinceLastRefreshDisplay = string.Empty;
    private string _sinceYesterdayDisplay = string.Empty;

    [DataMember] public bool HasUsageTrend { get => _hasUsageTrend; set => SetField(ref _hasUsageTrend, value); }
    [DataMember] public bool HasChart { get => _hasChart; set => SetField(ref _hasChart, value); }
    [DataMember] public bool HasComparisons { get => _hasComparisons; set => SetField(ref _hasComparisons, value); }
    [DataMember] public bool HasSinceLastRefresh { get => _hasSinceLastRefresh; set => SetField(ref _hasSinceLastRefresh, value); }
    [DataMember] public bool HasSinceYesterday { get => _hasSinceYesterday; set => SetField(ref _hasSinceYesterday, value); }
    [DataMember] public string SinceLastRefreshDisplay { get => _sinceLastRefreshDisplay; set => SetField(ref _sinceLastRefreshDisplay, value); }
    [DataMember] public string SinceYesterdayDisplay { get => _sinceYesterdayDisplay; set => SetField(ref _sinceYesterdayDisplay, value); }

    [DataMember]
    public ObservableCollection<ChartBarViewModel> ChartBars { get; } = [];

    // Weighted prediction
    private bool _hasPrediction;
    private string _predictedDailyUsageDisplay = string.Empty;
    private string _predictionConfidenceLabel = string.Empty;
    private string _predictionConfidenceReason = string.Empty;
    private string _predictionDailyCost = string.Empty;
    private string _predictionMonthlyCost = string.Empty;
    private bool _hasExhaustion;
    private string _daysUntilExhaustionDisplay = string.Empty;
    private string _sustainabilityDisplay = string.Empty;

    [DataMember] public bool HasPrediction { get => _hasPrediction; set => SetField(ref _hasPrediction, value); }
    [DataMember] public string PredictedDailyUsageDisplay { get => _predictedDailyUsageDisplay; set => SetField(ref _predictedDailyUsageDisplay, value); }
    [DataMember] public string PredictionConfidenceLabel { get => _predictionConfidenceLabel; set => SetField(ref _predictionConfidenceLabel, value); }
    [DataMember] public string PredictionConfidenceReason { get => _predictionConfidenceReason; set => SetField(ref _predictionConfidenceReason, value); }
    [DataMember] public string PredictionDailyCost { get => _predictionDailyCost; set => SetField(ref _predictionDailyCost, value); }
    [DataMember] public string PredictionMonthlyCost { get => _predictionMonthlyCost; set => SetField(ref _predictionMonthlyCost, value); }
    [DataMember] public bool HasExhaustion { get => _hasExhaustion; set => SetField(ref _hasExhaustion, value); }
    [DataMember] public string DaysUntilExhaustionDisplay { get => _daysUntilExhaustionDisplay; set => SetField(ref _daysUntilExhaustionDisplay, value); }
    [DataMember] public string SustainabilityDisplay { get => _sustainabilityDisplay; set => SetField(ref _sustainabilityDisplay, value); }

    // Burn-rate / trend analysis
    private bool _hasBurnRate;
    private string _recentBurnRateDisplay = string.Empty;
    private string _overallBurnRateDisplay = string.Empty;
    private string _projectedMonthlyCostDisplay = string.Empty;
    private string _trendIndicatorDisplay = string.Empty;

    [DataMember] public bool HasBurnRate { get => _hasBurnRate; set => SetField(ref _hasBurnRate, value); }
    [DataMember] public string RecentBurnRateDisplay { get => _recentBurnRateDisplay; set => SetField(ref _recentBurnRateDisplay, value); }
    [DataMember] public string OverallBurnRateDisplay { get => _overallBurnRateDisplay; set => SetField(ref _overallBurnRateDisplay, value); }
    [DataMember] public string ProjectedMonthlyCostDisplay { get => _projectedMonthlyCostDisplay; set => SetField(ref _projectedMonthlyCostDisplay, value); }
    [DataMember] public string TrendIndicatorDisplay { get => _trendIndicatorDisplay; set => SetField(ref _trendIndicatorDisplay, value); }

    // Settings + export status
    private string _customCreditLimitInput = string.Empty;
    private string _pollingIntervalInput = string.Empty;
    private string _settingsStatus = string.Empty;
    private string _exportStatus = string.Empty;

    [DataMember] public string CustomCreditLimitInput { get => _customCreditLimitInput; set => SetField(ref _customCreditLimitInput, value); }
    [DataMember] public string PollingIntervalInput { get => _pollingIntervalInput; set => SetField(ref _pollingIntervalInput, value); }
    [DataMember] public string SettingsStatus { get => _settingsStatus; set => SetField(ref _settingsStatus, value); }
    [DataMember] public string ExportStatus { get => _exportStatus; set => SetField(ref _exportStatus, value); }

    [DataMember]
    public ObservableCollection<QuotaCardViewModel> Quotas { get; } = [];

    [DataMember]
    public IAsyncCommand RefreshCommand { get; }

    [DataMember]
    public IAsyncCommand CopyMarkdownCommand { get; }

    [DataMember]
    public IAsyncCommand CopyJsonCommand { get; }

    [DataMember]
    public IAsyncCommand SaveSettingsCommand { get; }

    [DataMember]
    public IAsyncCommand ResetSettingsCommand { get; }

    public CopilotInsightsViewModel(
        GitHubCopilotService service,
        VisualStudioExtensibility extensibility,
        LocalStorageService storage)
    {
        _service = service;
        _extensibility = extensibility;
        _storage = storage;
        _snapshotHistory = storage.LoadSnapshots();
        _settings = storage.LoadSettings();

        RefreshCommand = new AsyncCommand((_, ct) => RefreshAsync(silent: false, ct));
        CopyMarkdownCommand = new AsyncCommand(OnCopyMarkdownAsync);
        CopyJsonCommand = new AsyncCommand(OnCopyJsonAsync);
        SaveSettingsCommand = new AsyncCommand(OnSaveSettingsAsync);
        ResetSettingsCommand = new AsyncCommand(OnResetSettingsAsync);

        SyncSettingsInputs();
        _ = RefreshAsync(silent: false, _initialLoadCts.Token);
        RestartPolling();
    }

    private async Task RefreshAsync(bool silent, CancellationToken cancellationToken)
    {
        // Serialize refreshes so a background poll and a manual refresh never mutate the
        // bound collections concurrently. A busy silent poll is simply skipped.
        if (silent)
        {
            if (!await _refreshGate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            {
                return;
            }
        }
        else
        {
            await _refreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            if (!silent)
            {
                IsLoading = true;
                HasError = false;
                HasData = false;
                ErrorMessage = string.Empty;
            }

            try
            {
                CopilotUserData data = await _service.GetCopilotUserDataAsync(cancellationToken);
                _latestData = data;
                RecordSnapshot(data);
                PopulateFromData(data);
                HasError = false;
                ErrorMessage = string.Empty;
                HasData = true;
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    HasError = true;
                    ErrorMessage = ex.Message;
                }
            }
            finally
            {
                if (!silent)
                {
                    IsLoading = false;
                }
            }
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    private void RecordSnapshot(CopilotUserData data)
    {
        QuotaSnapshot? premium = data.QuotaSnapshots?.Values
            .FirstOrDefault(q => q.QuotaId == PremiumQuotaId);

        if (premium is null || premium.Unlimited)
        {
            return;
        }

        if (UsageAnalytics.AddSnapshot(_snapshotHistory, premium.Remaining, premium.Entitlement))
        {
            _storage.SaveSnapshots(_snapshotHistory);
        }
    }

    private void PopulateFromData(CopilotUserData data)
    {
        DateTime? resetDateUtc = null;
        TimeSpan? timeUntilReset = null;
        if (!string.IsNullOrWhiteSpace(data.QuotaResetDateUtc) &&
            DateTime.TryParse(data.QuotaResetDateUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedResetDate))
        {
            resetDateUtc = parsedResetDate;
            timeUntilReset = parsedResetDate - DateTime.UtcNow;
        }

        // Plan details
        PlanValue = string.IsNullOrWhiteSpace(data.CopilotPlan) ? "Unknown" : QuotaDisplayFormatter.NormalizePlan(data.CopilotPlan);
        ChatValue = data.ChatEnabled ? "Enabled" : "Disabled";
        SkuValue = string.IsNullOrWhiteSpace(data.AccessTypeSku) ? "N/A" : data.AccessTypeSku;

        // Organizations
        List<Organization> orgs = data.OrganizationList ?? [];
        if (orgs.Count > 0)
        {
            string orgNames = string.Join(", ", orgs.Select(o => string.IsNullOrWhiteSpace(o.Name) ? o.Login : o.Name));
            OrgValue = $"{orgs.Count} ({orgNames})";
        }
        else
        {
            OrgValue = "None";
        }

        // Quotas
        Quotas.Clear();
        Dictionary<string, QuotaSnapshot> snapshots = data.QuotaSnapshots ?? [];

        // Sort so premium quota comes first
        var sorted = snapshots.Values
            .OrderByDescending(q => q.QuotaId == PremiumQuotaId)
            .ThenBy(q => q.QuotaId);

        QuotaSnapshot? premiumQuota = null;

        foreach (QuotaSnapshot rawQuota in sorted)
        {
            QuotaSnapshot quota = rawQuota.QuotaId == PremiumQuotaId
                ? GetEffectiveQuota(rawQuota, _settings.CustomCreditLimit)
                : rawQuota;

            string name = QuotaDisplayFormatter.FormatQuotaName(quota.QuotaId);
            var card = new QuotaCardViewModel { Name = name };

            if (quota.Unlimited)
            {
                card.StatusEmoji = "∞";
                card.StatusLabel = "Unlimited";
                card.UsageDisplay = "Unlimited usage";
                card.PercentDisplay = string.Empty;
                card.ProgressValue = 0;
                card.IsLimited = false;
            }
            else
            {
                double used = quota.Entitlement - quota.QuotaRemaining;
                double percentRemaining = quota.Entitlement > 0
                    ? Math.Round(quota.QuotaRemaining / quota.Entitlement * 100, 1)
                    : 0;
                double percentUsed = quota.Entitlement > 0
                    ? Math.Round(used / quota.Entitlement * 100, 1)
                    : 0;

                bool isOver = quota.Remaining < 0;
                double overAmount = isOver ? Math.Abs(quota.Remaining) : 0;
                card.IsOverQuota = isOver;
                card.IsLimited = true;
                (card.StatusEmoji, card.StatusLabel) = QuotaDisplayFormatter.GetStatusBadge(percentRemaining);
                card.ProgressValue = Math.Clamp(percentRemaining, 0, 100);

                if (isOver)
                {
                    card.UsageDisplay = $"{QuotaDisplayFormatter.Format(used)} / {QuotaDisplayFormatter.Format(quota.Entitlement)} used";
                    card.PercentDisplay = $"{percentUsed}% used";
                    double billableOverage = Math.Max(0, used - quota.Entitlement);
                    double estimatedCost = billableOverage * CreditCostUsd;
                    card.OverageDisplay = $"Over by {QuotaDisplayFormatter.Format(overAmount)} credits" +
                        (quota.OveragePermitted
                            ? $" (est. cost: ${estimatedCost:F2})"
                            : " (overage NOT permitted)");
                }
                else
                {
                    card.UsageDisplay = $"{QuotaDisplayFormatter.Format(quota.QuotaRemaining)} / {QuotaDisplayFormatter.Format(quota.Entitlement)} remaining";
                    card.PercentDisplay = $"{percentRemaining}% remaining";
                }

                if (quota.QuotaId == PremiumQuotaId)
                {
                    premiumQuota = quota;
                    card.ShowPremiumStats = true;
                    card.PremiumMood = QuotaDisplayFormatter.GetMood(percentRemaining);
                    card.PremiumRemainingPercent = $"{Math.Max(0, percentRemaining)}% remaining";
                    card.PremiumRemaining = QuotaDisplayFormatter.Format(Math.Max(quota.QuotaRemaining, 0));
                    card.PremiumUsed = QuotaDisplayFormatter.Format(used);
                    card.PremiumTotal = QuotaDisplayFormatter.Format(quota.Entitlement);

                    if (timeUntilReset is { } diff && resetDateUtc is { } resetAt && diff.TotalDays > 0)
                    {
                        PacingBudget budget = ComputePacingBudget(Math.Max(quota.QuotaRemaining, 0), diff.TotalDays);
                        card.PremiumAllowancePerDay = $"\u2264 {budget.Daily}/day";
                        card.PremiumResetIn = QuotaDisplayFormatter.FormatResetInShort(diff);
                        card.PremiumResetDate = resetAt.ToLocalTime().ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                        card.PremiumWeeklyAverage = $"\u2264 {budget.Weekly}/week";
                        card.PremiumWorkdayAverage = $"\u2264 {budget.PerWorkday}/day (Mon-Fri)";
                        card.PremiumWorkhourAverage = $"\u2264 {budget.PerHour}/hour (9-5)";
                        card.PremiumEfficientCapacity = $"~{budget.BudgetEfficient}/day";
                        card.PremiumStandardCapacity = $"~{budget.BudgetStandard}/day";
                        card.PremiumAdvancedCapacity = $"~{budget.BudgetAdvanced}/day";
                    }

                    double billableOverage = Math.Max(0, used - quota.Entitlement);
                    card.PremiumOveragePolicy = quota.OveragePermitted
                        ? isOver
                            ? $"Overage permitted ({QuotaDisplayFormatter.Format(overAmount)} over, est. cost: ${(billableOverage * CreditCostUsd):F2})"
                            : quota.OverageCount > 0
                                ? $"Overage permitted ({QuotaDisplayFormatter.Format(quota.OverageCount)} used)"
                                : "Overage permitted"
                        : "Overage not permitted";
                }
            }

            Quotas.Add(card);
        }

        // Reset timing
        if (resetDateUtc is { } resetDate)
        {
            TimeSpan diff = resetDate - DateTime.UtcNow;
            if (diff.TotalSeconds > 0)
            {
                int days = (int)diff.TotalDays;
                int hours = diff.Hours;
                ResetCountdown = $"Resets in {days} day{(days != 1 ? "s" : "")}, {hours} hour{(hours != 1 ? "s" : "")}";
            }
            else
            {
                ResetCountdown = "Quota has reset";
            }

            ResetDateDisplay = $"Reset date: {resetDate.ToLocalTime():MMMM d, yyyy h:mm tt}";

            // Pacing guidance (only for premium interactions with limited quota)
            if (premiumQuota is not null && !premiumQuota.Unlimited && diff.TotalDays > 0 && premiumQuota.QuotaRemaining > 0)
            {
                PacingBudget budget = ComputePacingBudget(Math.Max(premiumQuota.QuotaRemaining, 0), diff.TotalDays);
                DailyValue = $"\u2264 {budget.Daily}/day";
                WeeklyValue = $"\u2264 {budget.Weekly}/week";
                WorkdayValue = $"\u2264 {budget.PerWorkday}/day (Mon-Fri)";
                HasPacing = true;
            }
            else
            {
                HasPacing = false;
            }
        }
        else
        {
            ResetCountdown = "Reset date unavailable";
            ResetDateDisplay = string.Empty;
            HasPacing = false;
        }

        PopulateAnalytics(premiumQuota, resetDateUtc);
        MaybeNotifyPremiumUsage(data, premiumQuota);
    }

    private void PopulateAnalytics(QuotaSnapshot? effectivePremium, DateTime? resetDateUtc)
    {
        // Delta comparisons
        SnapshotComparison comparison = UsageAnalytics.GetComparisons(_snapshotHistory);

        HasSinceLastRefresh = comparison.SinceLastRefresh is not null;
        SinceLastRefreshDisplay = QuotaDisplayFormatter.FormatDelta(comparison.SinceLastRefresh);

        HasSinceYesterday = comparison.SinceYesterday is not null;
        SinceYesterdayDisplay = QuotaDisplayFormatter.FormatDelta(comparison.SinceYesterday);

        HasComparisons = HasSinceLastRefresh || HasSinceYesterday;

        // Trend chart
        BuildChart();

        HasUsageTrend = HasChart || HasComparisons;

        // Weighted prediction
        double? effectiveRemaining = effectivePremium is { Unlimited: false } p ? p.Remaining : null;
        WeightedPrediction? prediction = UsageAnalytics.GetWeightedPrediction(_snapshotHistory, effectiveRemaining, resetDateUtc);
        if (prediction is not null)
        {
            HasPrediction = true;
            PredictedDailyUsageDisplay = prediction.PredictedDailyUsage.ToString(CultureInfo.InvariantCulture);
            PredictionConfidenceLabel = prediction.Confidence switch
            {
                "high" => "High Accuracy",
                "medium" => "Medium Accuracy",
                _ => "Low Accuracy",
            };
            PredictionConfidenceReason = prediction.ConfidenceReason;
            PredictionDailyCost = $"~${prediction.PredictedDailyUsage * CreditCostUsd:F2}/day";
            PredictionMonthlyCost = $"~${prediction.PredictedDailyUsage * CreditCostUsd * DaysPerMonth:F2}";
            HasExhaustion = prediction.DaysUntilExhaustion is not null;
            DaysUntilExhaustionDisplay = prediction.DaysUntilExhaustion is { } days ? $"{days} days" : string.Empty;
            SustainabilityDisplay = prediction.WillExhaustBeforeReset
                ? "\u26A0 May exhaust before reset"
                : "\u2713 On track for reset";
        }
        else
        {
            HasPrediction = false;
        }

        // Burn-rate / trend analysis
        TrendPrediction? trend = UsageAnalytics.GetTrendPrediction(_snapshotHistory);
        if (trend is not null)
        {
            HasBurnRate = true;
            RecentBurnRateDisplay = $"{trend.RecentBurnRate} credits/day (~${trend.RecentBurnRate * CreditCostUsd:F2}/day)";
            OverallBurnRateDisplay = $"{trend.OverallBurnRate} credits/day (~${trend.OverallBurnRate * CreditCostUsd:F2}/day)";
            ProjectedMonthlyCostDisplay = $"~${trend.RecentBurnRate * CreditCostUsd * DaysPerMonth:F2}";
            TrendIndicatorDisplay = trend.TrendIndicator;
        }
        else
        {
            HasBurnRate = false;
        }
    }

    private void BuildChart()
    {
        ChartBars.Clear();

        const int maxBars = 30;
        const double maxHeight = 56;

        List<LocalSnapshot> valid = _snapshotHistory.Where(s => s.PremiumEntitlement > 0).ToList();
        if (valid.Count < 2)
        {
            HasChart = false;
            return;
        }

        // _snapshotHistory is newest-first; take the most recent bars oldest-to-newest.
        List<LocalSnapshot> recent = valid.Take(maxBars).Reverse().ToList();

        double minV = recent.Min(s => s.PremiumRemaining);
        double maxV = recent.Max(s => s.PremiumRemaining);
        double range = maxV - minV;
        if (range == 0)
        {
            range = 1;
        }

        double yMin = minV - range * 0.1;
        double yMax = maxV + range * 0.1;
        double yRange = yMax - yMin;
        if (yRange == 0)
        {
            yRange = 1;
        }

        foreach (LocalSnapshot snapshot in recent)
        {
            double normalized = (snapshot.PremiumRemaining - yMin) / yRange;
            double height = Math.Clamp(normalized * maxHeight, 1, maxHeight);
            ChartBars.Add(new ChartBarViewModel
            {
                Height = height,
                Tooltip = $"{QuotaDisplayFormatter.Format(snapshot.PremiumRemaining)} credits \u00B7 {QuotaDisplayFormatter.FormatSnapshotTime(snapshot.Timestamp)}",
            });
        }

        HasChart = true;
    }

    private void MaybeNotifyPremiumUsage(CopilotUserData data, QuotaSnapshot? effectivePremium)
    {
        if (effectivePremium is null || effectivePremium.Unlimited || effectivePremium.Entitlement <= 0)
        {
            return;
        }

        double used = effectivePremium.Entitlement - effectivePremium.QuotaRemaining;
        double percentUsed = Math.Round(used / effectivePremium.Entitlement * 100, 1);

        double planEntitlement = data.QuotaSnapshots?.Values
            .FirstOrDefault(q => q.QuotaId == PremiumQuotaId)?.Entitlement
            ?? effectivePremium.Entitlement;
        bool hasCustomLimit = effectivePremium.Entitlement > planEntitlement;
        string quotaLabel = hasCustomLimit ? "custom limit" : "monthly quota";

        string resetDate = data.QuotaResetDateUtc ?? string.Empty;
        string? lastNotified = _storage.GetLastNotifiedReset();

        if (percentUsed >= PremiumUsageAlertThreshold && lastNotified != resetDate)
        {
            string message = $"Copilot AI Credits are at {QuotaDisplayFormatter.Format(percentUsed)}% of your {quotaLabel}.";
            _ = ShowAlertAsync(message);
            _storage.SetLastNotifiedReset(resetDate);
        }
        else if (!string.IsNullOrEmpty(lastNotified) && lastNotified != resetDate && percentUsed < PremiumUsageAlertThreshold)
        {
            _storage.SetLastNotifiedReset(null);
        }
    }

    private async Task ShowAlertAsync(string message)
    {
        try
        {
            await _extensibility.Shell().ShowPromptAsync(message, PromptOptions.WarningConfirm, CancellationToken.None);
        }
        catch
        {
            // Notifications are best-effort.
        }
    }

    private async Task OnCopyMarkdownAsync(object? parameter, CancellationToken cancellationToken)
    {
        if (_latestData is null)
        {
            ExportStatus = "No data to export yet.";
            return;
        }

        string markdown = MarkdownExporter.BuildMarkdown(_latestData, _settings.CustomCreditLimit);
        ExportStatus = ClipboardHelper.SetText(markdown)
            ? "Markdown summary copied to clipboard."
            : "Could not access the clipboard.";

        await Task.CompletedTask;
    }

    private async Task OnCopyJsonAsync(object? parameter, CancellationToken cancellationToken)
    {
        if (_latestData is null)
        {
            ExportStatus = "No data to export yet.";
            return;
        }

        string json = MarkdownExporter.BuildJson(_latestData);
        ExportStatus = ClipboardHelper.SetText(json)
            ? "Raw JSON copied to clipboard."
            : "Could not access the clipboard.";

        await Task.CompletedTask;
    }

    private async Task OnSaveSettingsAsync(object? parameter, CancellationToken cancellationToken)
    {
        double customLimit = 0;
        if (!string.IsNullOrWhiteSpace(CustomCreditLimitInput)
            && (!double.TryParse(CustomCreditLimitInput.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out customLimit) || customLimit < 0))
        {
            SettingsStatus = "Custom credit limit must be a non-negative number.";
            return;
        }

        int pollingSeconds = InsightsSettings.DefaultPollingIntervalSeconds;
        if (!string.IsNullOrWhiteSpace(PollingIntervalInput)
            && (!int.TryParse(PollingIntervalInput.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out pollingSeconds) || pollingSeconds < 0))
        {
            SettingsStatus = "Auto-refresh interval must be 0 or a positive whole number of seconds.";
            return;
        }

        _settings = new InsightsSettings
        {
            CustomCreditLimit = customLimit,
            PollingIntervalSeconds = NormalizePollingIntervalSeconds(pollingSeconds),
        };
        _storage.SaveSettings(_settings);
        SyncSettingsInputs();
        SettingsStatus = "Settings saved.";

        RestartPolling();

        await RepopulateAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task OnResetSettingsAsync(object? parameter, CancellationToken cancellationToken)
    {
        _settings = new InsightsSettings();
        _storage.SaveSettings(_settings);
        SyncSettingsInputs();
        SettingsStatus = "Settings reset to defaults.";

        RestartPolling();

        await RepopulateAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Re-applies the current settings to the latest data under the refresh gate.</summary>
    private async Task RepopulateAsync(CancellationToken cancellationToken)
    {
        if (_latestData is null)
        {
            return;
        }

        await _refreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // _latestData won't change while we hold _refreshGate;
            // it is only written inside RefreshAsync, which also acquires the gate.
            PopulateFromData(_latestData);
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    private void SyncSettingsInputs()
    {
        CustomCreditLimitInput = _settings.CustomCreditLimit > 0
            ? _settings.CustomCreditLimit.ToString("0.##", CultureInfo.InvariantCulture)
            : "0";
        PollingIntervalInput = _settings.PollingIntervalSeconds.ToString(CultureInfo.InvariantCulture);
    }

    private void RestartPolling()
    {
        CancellationToken token;
        lock (_pollingGate)
        {
            _pollingCts?.Cancel();
            _pollingCts?.Dispose();
            _pollingCts = null;

            int seconds = NormalizePollingIntervalSeconds(_settings.PollingIntervalSeconds);
            if (seconds <= 0)
            {
                return;
            }

            var cts = new CancellationTokenSource();
            _pollingCts = cts;
            token = cts.Token;
            _ = PollLoopAsync(seconds, token);
        }
    }

    private async Task PollLoopAsync(int seconds, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds), token);
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await RefreshAsync(silent: true, token);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on disposal or settings change.
        }
    }

    private static int NormalizePollingIntervalSeconds(int value) => Math.Max(0, value);

    private static PacingBudget ComputePacingBudget(double remaining, double totalDays)
    {
        long daily = (long)Math.Floor(remaining / totalDays);
        double weeksRemaining = Math.Max(1, totalDays / 7.0);
        long weekly = (long)Math.Floor(remaining / weeksRemaining);
        long workingDays = (long)Math.Floor(totalDays * 5.0 / 7.0);
        long perWorkday = workingDays > 0 ? (long)Math.Floor(remaining / workingDays) : 0;
        long totalWorkingHours = workingDays * WorkingHoursPerDay;
        long perHour = totalWorkingHours > 0 ? (long)Math.Floor(remaining / totalWorkingHours) : 0;
        long budgetEfficient = (long)Math.Floor(remaining / EfficientModelCostMultiplier / totalDays);
        long budgetStandard = (long)Math.Floor(remaining / totalDays);
        long budgetAdvanced = (long)Math.Floor(remaining / AdvancedModelCostMultiplier / totalDays);
        return new PacingBudget(daily, weekly, perWorkday, perHour, budgetEfficient, budgetStandard, budgetAdvanced);
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
                PercentRemaining = customLimit > 0 ? effectiveRemaining / customLimit * 100 : 0,
                Unlimited = quota.Unlimited,
                OveragePermitted = quota.OveragePermitted,
                OverageCount = quota.OverageCount,
            };
        }

        return quota;
    }

    public void Dispose()
    {
        _initialLoadCts.Cancel();
        _initialLoadCts.Dispose();

        lock (_pollingGate)
        {
            _pollingCts?.Cancel();
            _pollingCts?.Dispose();
            _pollingCts = null;
        }

        // _refreshGate is intentionally not disposed: a poll task may still be releasing it,
        // and SemaphoreSlim only requires disposal when its AvailableWaitHandle is used (it isn't).
    }
}
