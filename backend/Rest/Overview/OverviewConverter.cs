using Domain.Overview;
using Models.Overview;
using Rest.AccountingPeriods;
using Rest.Accounts;
using Rest.Funds;

namespace Rest.Overview;

/// <summary>
/// Converter class that handles converting overview queries and results to and from API models.
/// </summary>
public sealed class OverviewConverter(AccountingPeriodConverter accountingPeriodConverter)
{
    /// <summary>
    /// Converts the provided overview history range model to its domain value.
    /// </summary>
    public static OverviewHistoryRange ToDomain(OverviewHistoryRangeModel history) => history switch
    {
        OverviewHistoryRangeModel.ThreeMonths => OverviewHistoryRange.ThreeMonths,
        OverviewHistoryRangeModel.OneYear => OverviewHistoryRange.OneYear,
        OverviewHistoryRangeModel.FiveYears => OverviewHistoryRange.FiveYears,
        _ => throw new InvalidOperationException($"Unrecognized Overview History Range: {history}")
    };

    /// <summary>
    /// Converts the provided overview to an API model.
    /// </summary>
    public OverviewModel ToModel(OverviewSummary overview) => new()
    {
        AccountOpeningBalance = AccountConverter.ToModel(overview.History.AccountOpeningBalance),
        AccountClosingBalance = AccountConverter.ToModel(overview.History.AccountClosingBalance),
        AccountBalances = overview.History.AccountBalances.Select(ToModel).ToList(),
        FundClosingBalance = FundConverter.ToModel(overview.History.FundClosingBalance),
        FundBalances = overview.History.FundBalances.Select(ToModel).ToList(),
        Granularity = BalanceGranularityConverter.ToModel(overview.History.Granularity),
        CurrentPeriod = overview.CurrentPeriod == null ? null : ToModel(overview.CurrentPeriod),
        Attention = overview.Attention.Select(ToModel).ToList(),
    };

    /// <summary>
    /// Converts the provided date balance to an API model.
    /// </summary>
    private static OverviewDateBalanceModel ToModel(OverviewDateBalance balance) => new()
    {
        Date = balance.Date,
        TotalBalance = balance.TotalBalance,
    };

    /// <summary>
    /// Converts the provided current Accounting Period figures to an API model.
    /// </summary>
    private OverviewPeriodModel ToModel(OverviewPeriod period) => new()
    {
        AccountingPeriod = accountingPeriodConverter.ToModel(period.AccountingPeriod),
        ExpectedIncome = period.ExpectedIncome,
        ActualIncome = period.ActualIncome,
        ExpectedGoalContributions = period.ExpectedGoalContributions,
        ActualGoalContributions = period.ActualGoalContributions,
        TotalSpending = period.TotalSpending,
    };

    /// <summary>
    /// Converts the provided group of items needing attention to an API model.
    /// </summary>
    private static OverviewAttentionGroupModel ToModel(OverviewAttentionGroup group) => new()
    {
        Kind = group.Kind switch
        {
            OverviewAttentionKind.GoalsBelowMinimum => OverviewAttentionKindModel.GoalsBelowMinimum,
            OverviewAttentionKind.GoalsBehind => OverviewAttentionKindModel.GoalsBehind,
            OverviewAttentionKind.UnpostedTransactions => OverviewAttentionKindModel.UnpostedTransactions,
            OverviewAttentionKind.IncomeNotReceived => OverviewAttentionKindModel.IncomeNotReceived,
            _ => throw new InvalidOperationException($"Unrecognized Overview Attention Kind: {group.Kind}")
        },
        Count = group.Count,
        Amount = group.Amount,
        ItemCount = group.ItemCount,
        TopItems = group.TopItems.Select(item => new OverviewAttentionItemModel
        {
            Name = item.Name,
            Amount = item.Amount,
            Count = item.Count,
        }).ToList(),
        OldestDate = group.OldestDate,
    };
}
