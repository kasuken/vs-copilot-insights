using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.Extensibility.UI;
using vs_copilot_insights.Models;
using vs_copilot_insights.Services;

namespace vs_copilot_insights;

[DataContract]
internal sealed class QuotaCardViewModel : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

[DataContract]
internal sealed class CopilotInsightsViewModel : INotifyPropertyChanged
{
    private readonly GitHubCopilotService _service;

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

    [DataMember]
    public ObservableCollection<QuotaCardViewModel> Quotas { get; } = [];

    [DataMember]
    public IAsyncCommand RefreshCommand { get; }

    public CopilotInsightsViewModel(GitHubCopilotService service)
    {
        _service = service;
        RefreshCommand = new AsyncCommand(OnRefreshAsync);
        _ = OnRefreshAsync(null, CancellationToken.None);
    }

    private async Task OnRefreshAsync(object? parameter, CancellationToken cancellationToken)
    {
        IsLoading = true;
        HasError = false;
        HasData = false;
        ErrorMessage = string.Empty;

        try
        {
            CopilotUserData data = await _service.GetCopilotUserDataAsync(cancellationToken);
            PopulateFromData(data);
            HasData = true;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
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
        PlanValue = string.IsNullOrWhiteSpace(data.CopilotPlan) ? "Unknown" : NormalizePlan(data.CopilotPlan);
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

        // Sort so premium_interactions comes first
        var sorted = snapshots.Values
            .OrderByDescending(q => q.QuotaId == "premium_interactions")
            .ThenBy(q => q.QuotaId);

        QuotaSnapshot? premiumQuota = null;

        foreach (QuotaSnapshot quota in sorted)
        {
            string name = FormatQuotaName(quota.QuotaId);
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
                (card.StatusEmoji, card.StatusLabel) = GetStatusBadge(percentRemaining);
                card.ProgressValue = Math.Clamp(percentUsed, 0, 100);

                if (isOver)
                {
                    card.UsageDisplay = $"{Format(used)} / {Format(quota.Entitlement)} used";
                    card.PercentDisplay = $"{percentUsed}% used";
                    double estimatedCost = overAmount * 0.04;
                    card.OverageDisplay = $"Over by {Format(overAmount)} requests" +
                        (quota.OveragePermitted
                            ? $" (est. cost: ${estimatedCost:F2})"
                            : " (overage NOT permitted)");
                }
                else
                {
                    card.UsageDisplay = $"{Format(quota.QuotaRemaining)} / {Format(quota.Entitlement)} remaining";
                    card.PercentDisplay = $"{percentRemaining}% remaining";
                }

                if (quota.QuotaId == "premium_interactions")
                {
                    premiumQuota = quota;
                    card.ShowPremiumStats = true;
                    card.PremiumMood = GetMood(percentRemaining);
                    card.PremiumRemainingPercent = $"{Math.Max(0, percentRemaining):F1}% remaining";
                    card.PremiumRemaining = Format(Math.Max(quota.QuotaRemaining, 0));
                    card.PremiumUsed = Format(used);
                    card.PremiumTotal = Format(quota.Entitlement);

                    if (timeUntilReset is { } diff && resetDateUtc is { } resetAt && diff.TotalHours > 0)
                    {
                        double daysLeft = Math.Max(diff.TotalDays, 0.01);
                        double premiumRemaining = Math.Max(quota.QuotaRemaining, 0);

                        double dailyAllowance = premiumRemaining / daysLeft;
                        double weeklyAverage = dailyAllowance * 7;
                        double workdaysLeft = Math.Max(daysLeft * 5.0 / 7.0, 0.01);
                        double workdayAverage = premiumRemaining / workdaysLeft;
                        double workhoursLeft = Math.Max(workdaysLeft * 8.0, 0.01);
                        double workhourAverage = premiumRemaining / workhoursLeft;

                        card.PremiumAllowancePerDay = FormatAtMostPerPeriod(dailyAllowance, "day");
                        card.PremiumResetIn = FormatResetInShort(diff);
                        card.PremiumResetDate = resetAt.ToLocalTime().ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

                        card.PremiumWeeklyAverage = FormatAtMostPerPeriod(weeklyAverage, "week");
                        card.PremiumWorkdayAverage = FormatAtMostPerPeriod(workdayAverage, "day (Mon-Fri)");
                        card.PremiumWorkhourAverage = FormatAtMostPerPeriod(workhourAverage, "hour (9-5)");

                        card.PremiumEfficientCapacity = FormatApproxPerDay(dailyAllowance / 0.33);
                        card.PremiumStandardCapacity = FormatApproxPerDay(dailyAllowance);
                        card.PremiumAdvancedCapacity = FormatApproxPerDay(dailyAllowance / 3.0);
                    }

                    card.PremiumOveragePolicy = quota.OveragePermitted
                        ? isOver
                            ? $"Overage permitted ({Format(overAmount)} over, est. cost: ${(overAmount * 0.04):F2})"
                            : quota.OverageCount > 0
                                ? $"Overage permitted ({Format(quota.OverageCount)} used)"
                                : "Overage permitted"
                        : "Overage not permitted";
                }
            }

            Quotas.Add(card);
        }

        // Reset timing
        if (!string.IsNullOrWhiteSpace(data.QuotaResetDateUtc) &&
            DateTime.TryParse(data.QuotaResetDateUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime resetDate))
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
                double daysLeft = diff.TotalDays;
                double daily = premiumQuota.QuotaRemaining / daysLeft;
                double weekly = daily * 7;
                double workdaysLeft = daysLeft * 5.0 / 7.0;
                double perWorkday = workdaysLeft > 0 ? premiumQuota.QuotaRemaining / workdaysLeft : 0;

                DailyValue = $"~{daily:F1} requests";
                WeeklyValue = $"~{weekly:F1} requests";
                WorkdayValue = $"~{perWorkday:F1} requests";
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
    }

    private static string NormalizePlan(string plan)
    {
        string trimmed = plan.Trim();
        return trimmed.Length > 0
            ? char.ToUpper(trimmed[0]) + trimmed[1..]
            : trimmed;
    }

    private static string FormatQuotaName(string quotaId)
    {
        return CultureInfo.InvariantCulture.TextInfo
            .ToTitleCase(quotaId.Replace('_', ' '));
    }

    private static (string Emoji, string Label) GetStatusBadge(double percentRemaining)
    {
        return percentRemaining switch
        {
            <= 0 => ("✖", "Over Quota"),
            > 50 => ("✔", "Healthy"),
            >= 20 => ("⚠", "Watch"),
            _ => ("▲", "Risk"),
        };
    }

    private static string GetMood(double percentRemaining)
    {
        return percentRemaining switch
        {
            <= 0 => "Over quota",
            <= 15 => "Getting tight",
            <= 40 => "Watch usage",
            <= 75 => "You are fine",
            _ => "Comfortable",
        };
    }

    private static string Format(double value)
    {
        return value == Math.Floor(value)
            ? value.ToString("F0", CultureInfo.InvariantCulture)
            : value.ToString("F1", CultureInfo.InvariantCulture);
    }

    private static string FormatAtMostPerPeriod(double value, string period)
    {
        return $"≤ {Math.Max(0, Math.Round(value)):F0}/{period}";
    }

    private static string FormatApproxPerDay(double value)
    {
        return $"~{Math.Max(0, Math.Round(value)):F0}/day";
    }

    private static string FormatResetInShort(TimeSpan diff)
    {
        int days = Math.Max(0, (int)diff.TotalDays);
        int hours = Math.Max(0, diff.Hours);
        return $"{days}d {hours}h";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
