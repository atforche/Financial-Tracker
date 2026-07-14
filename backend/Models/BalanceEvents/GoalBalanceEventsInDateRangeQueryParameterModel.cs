using Models.Goals;

namespace Models.BalanceEvents;

/// <summary>
/// Model representing the query parameters that can be applied when retrieving Goal balance events in a date range.
/// </summary>
public class GoalBalanceEventsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Date range to apply to the results.
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Filters to apply to the results.
    /// </summary>
    public GoalFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort order to apply to the results.
    /// </summary>
    public GoalBalanceEventSortModel? Sort { get; init; }
}