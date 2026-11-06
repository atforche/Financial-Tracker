namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the earliest and latest Accounting Periods
/// </summary>
public class AccountingPeriodBoundsModel
{
    /// <summary>
    /// Earliest Accounting Period, or null when none exist
    /// </summary>
    public required AccountingPeriodModel? First { get; init; }

    /// <summary>
    /// Latest Accounting Period, or null when none exist
    /// </summary>
    public required AccountingPeriodModel? Latest { get; init; }
}
