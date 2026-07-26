namespace Models.FundGoals;

/// <summary>
/// Query parameters for Fund Goal balance events in a date range.
/// </summary>
public sealed class FundGoalBalanceEventsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Date range to query.
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Optional Fund Goal filter.
    /// </summary>
    public FundGoalFilterModel? Filter { get; init; }

    /// <summary>
    /// Optional event ordering.
    /// </summary>
    public FundGoalBalanceEventSortModel? Sort { get; init; }
}