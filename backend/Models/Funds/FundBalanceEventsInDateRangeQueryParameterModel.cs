namespace Models.Funds;

/// <summary>
/// Model representing the query parameters that can be applied when retrieving Fund balance events in a date range.
/// </summary>
public class FundBalanceEventsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Posting date range to apply to the results.
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Whether to include current pending events in addition to matching posted events.
    /// </summary>
    public bool IncludePending { get; init; }

    /// <summary>
    /// Filters to apply to the results.
    /// </summary>
    public FundFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort order to apply to the results.
    /// </summary>
    public FundBalanceEventSortModel? Sort { get; init; }
}
