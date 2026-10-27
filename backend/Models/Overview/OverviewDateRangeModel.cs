using Models.Accounts;
using Models.Funds;

namespace Models.Overview;

/// <summary>
/// Model representing Account and Fund balance summaries over a date range.
/// </summary>
public class OverviewDateRangeModel
{
    /// <summary>
    /// Account balances at the start of the range.
    /// </summary>
    public required AccountBalanceSummaryModel AccountOpeningBalance { get; init; }

    /// <summary>
    /// Account balances at the end of the range.
    /// </summary>
    public required AccountBalanceSummaryModel AccountClosingBalance { get; init; }

    /// <summary>
    /// Total Account balances for each date in the range.
    /// </summary>
    public required IReadOnlyCollection<OverviewDateBalanceModel> AccountBalances { get; init; }

    /// <summary>
    /// Fund balances at the start of the range.
    /// </summary>
    public required FundBalanceSummaryModel FundOpeningBalance { get; init; }

    /// <summary>
    /// Fund balances at the end of the range.
    /// </summary>
    public required FundBalanceSummaryModel FundClosingBalance { get; init; }

    /// <summary>
    /// Total Fund balances for each date in the range.
    /// </summary>
    public required IReadOnlyCollection<OverviewDateBalanceModel> FundBalances { get; init; }
}
