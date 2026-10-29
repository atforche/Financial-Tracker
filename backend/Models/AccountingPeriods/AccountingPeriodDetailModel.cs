using Models.Accounts;
using Models.Funds;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing everything shown for a single Accounting Period.
/// </summary>
public class AccountingPeriodDetailModel
{
    /// <summary>
    /// The Accounting Period with its balance and totals.
    /// </summary>
    public required AccountingPeriodWithBalanceModel AccountingPeriod { get; init; }

    /// <summary>
    /// The number of Fund Goals whose expected contribution is less than planned because of their maximum ending balance.
    /// </summary>
    public required int CappedGoalCount { get; init; }

    /// <summary>
    /// The Accounting Period immediately before this one, or null when this is the first.
    /// </summary>
    public required AccountingPeriodModel? PreviousPeriod { get; init; }

    /// <summary>
    /// The Accounting Period immediately after this one, or null when this is the latest.
    /// </summary>
    public required AccountingPeriodModel? NextPeriod { get; init; }

    /// <summary>
    /// Posted spending, net of refunds, during the Accounting Period.
    /// </summary>
    public required decimal TotalSpending { get; init; }

    /// <summary>
    /// Account balances at the start of the Accounting Period.
    /// </summary>
    public required AccountBalanceSummaryModel AccountOpeningBalance { get; init; }

    /// <summary>
    /// Account balances at the end of the Accounting Period.
    /// </summary>
    public required AccountBalanceSummaryModel AccountClosingBalance { get; init; }

    /// <summary>
    /// Fund balances at the start of the Accounting Period.
    /// </summary>
    public required FundBalanceSummaryModel FundOpeningBalance { get; init; }

    /// <summary>
    /// Fund balances at the end of the Accounting Period.
    /// </summary>
    public required FundBalanceSummaryModel FundClosingBalance { get; init; }
}
