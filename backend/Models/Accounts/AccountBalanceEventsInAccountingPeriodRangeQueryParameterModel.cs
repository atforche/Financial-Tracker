namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters that can be applied when retrieving Account balance events in an accounting period range
/// </summary>
public class AccountBalanceEventsInAccountingPeriodRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Accounting period range to apply to the results
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Filters to apply to the results
    /// </summary>
    public AccountFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountBalanceEventSortModel? Sort { get; init; }
}
