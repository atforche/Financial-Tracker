namespace Models;

/// <summary>
/// Model representing a range of Accounting Periods.
/// </summary>
public class AccountingPeriodRangeModel
{
    /// <summary>
    /// ID for the first Accounting Period in the range.
    /// </summary>
    public required Guid Start { get; init; }

    /// <summary>
    /// ID for the last Accounting Period in the range.
    /// </summary>
    public required Guid End { get; init; }
}