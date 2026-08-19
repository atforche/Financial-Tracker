using Models.AccountingPeriods;

namespace Models.Accounts;

/// <summary>
/// Model representing a summary of account balances for a specific Accounting Period.
/// </summary>
public class AccountBalanceSummaryByPeriodModel
{
    /// <summary>
    /// Accounting Period for this summary.
    /// </summary>
    public required AccountingPeriodModel AccountingPeriod { get; init; }

    /// <summary>
    /// Opening balance summary for this period.
    /// </summary>
    public required AccountBalanceSummaryModel OpeningBalance { get; init; }

    /// <summary>
    /// Closing balance summary for this period.
    /// </summary>
    public required AccountBalanceSummaryModel ClosingBalance { get; init; }
}
