namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the Accounting Periods in a requested range.
/// </summary>
public class AccountingPeriodsInRangeModel
{
    /// <summary>
    /// Accounting Periods in the requested range, in date order.
    /// </summary>
    public required IReadOnlyCollection<AccountingPeriodWithBalanceModel> AccountingPeriods { get; init; }

    /// <summary>
    /// Total spending for the requested range.
    /// </summary>
    public required decimal TotalSpending { get; init; }
}
