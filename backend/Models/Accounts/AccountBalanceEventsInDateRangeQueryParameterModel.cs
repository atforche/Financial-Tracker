namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters that can be applied when retrieving Account balance events in a date range
/// </summary>
public class AccountBalanceEventsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Date range to apply to the results
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Filters to apply to the results
    /// </summary>
    public AccountFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountBalanceEventSortModel? Sort { get; init; }
}