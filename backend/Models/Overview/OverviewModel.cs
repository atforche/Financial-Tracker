using Models.Accounts;
using Models.Funds;

namespace Models.Overview;

/// <summary>
/// Model representing everything shown on the overview.
/// </summary>
public class OverviewModel
{
    /// <summary>
    /// Account balances at the start of the balance history.
    /// </summary>
    public required AccountBalanceSummaryModel AccountOpeningBalance { get; init; }

    /// <summary>
    /// Account balances as of the as-of date.
    /// </summary>
    public required AccountBalanceSummaryModel AccountClosingBalance { get; init; }

    /// <summary>
    /// Total Account balances over the balance history.
    /// </summary>
    public required IReadOnlyCollection<OverviewDateBalanceModel> AccountBalances { get; init; }

    /// <summary>
    /// Fund balances as of the as-of date.
    /// </summary>
    public required FundBalanceSummaryModel FundClosingBalance { get; init; }

    /// <summary>
    /// Total Fund balances over the balance history.
    /// </summary>
    public required IReadOnlyCollection<OverviewDateBalanceModel> FundBalances { get; init; }

    /// <summary>
    /// Spacing between the dates in the balance history.
    /// </summary>
    public required BalanceGranularityModel Granularity { get; init; }

    /// <summary>
    /// Figures for the most recent open Accounting Period, or null when none is open.
    /// </summary>
    public OverviewPeriodModel? CurrentPeriod { get; init; }

    /// <summary>
    /// Groups of items needing attention, only for groups that have items.
    /// </summary>
    public required IReadOnlyCollection<OverviewAttentionGroupModel> Attention { get; init; }
}
