using Models.AccountingPeriods;

namespace Models.Overview;

/// <summary>
/// Model representing total opening and closing balances for an Accounting Period.
/// </summary>
public class OverviewPeriodBalanceModel
{
    /// <summary>
    /// Accounting Period for the balances.
    /// </summary>
    public required AccountingPeriodModel AccountingPeriod { get; init; }

    /// <summary>
    /// Total posted balance at the start of the Accounting Period.
    /// </summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>
    /// Total posted balance at the end of the Accounting Period.
    /// </summary>
    public required decimal ClosingBalance { get; init; }
}
