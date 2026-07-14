using Models.Funds;

namespace Models.BalanceEvents;

/// <summary>
/// Model representing the query parameters that can be applied when retrieving Fund balance events in a date range.
/// </summary>
public class FundBalanceEventsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Date range to apply to the results.
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Filters to apply to the results.
    /// </summary>
    public FundFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort order to apply to the results.
    /// </summary>
    public FundBalanceEventSortModel? Sort { get; init; }
}