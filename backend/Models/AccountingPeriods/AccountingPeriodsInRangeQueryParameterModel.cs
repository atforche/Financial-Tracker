namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters for getting a range of Accounting Periods with their balances.
/// </summary>
public class AccountingPeriodsInRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Range of Accounting Periods to include in the query.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Accounting Periods.
    /// </summary>
    public AccountingPeriodWithBalanceSortModel? Sort { get; init; }
}
