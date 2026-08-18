using Models.AccountingPeriods;

namespace Models.Funds;

/// <summary>
/// Model representing a summary of fund balances for a specific Accounting Period.
/// </summary>
public class FundBalanceSummaryByPeriodModel
{
    /// <summary>
    /// Accounting Period for this summary.
    /// </summary>
    public required AccountingPeriodModel AccountingPeriod { get; init; }

    /// <summary>
    /// Opening balance summary for this period.
    /// </summary>
    public required FundBalanceSummaryModel OpeningBalance { get; init; }

    /// <summary>
    /// Closing balance summary for this period.
    /// </summary>
    public required FundBalanceSummaryModel ClosingBalance { get; init; }
}
