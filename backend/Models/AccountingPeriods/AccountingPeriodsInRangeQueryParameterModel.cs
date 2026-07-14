namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters for getting a range of Accounting Periods with their balances.
/// </summary>
public class AccountingPeriodsInRangeQueryParameterModel
{
    /// <summary>
    /// Range of Accounting Periods to include in the query.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Accounting Periods.
    /// </summary>
    public AccountingPeriodWithBalanceSortModel? Sort { get; init; }

    /// <summary>
    /// Pagination settings for the matching Accounting Periods.
    /// </summary>
    public PaginationModel? AccountingPeriodPagination { get; init; }
}