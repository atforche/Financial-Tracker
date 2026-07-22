namespace Models.FundPlans;

/// <summary>
/// Query parameters for Fund Plan balance events in a date range.
/// </summary>
public sealed class FundPlanBalanceEventsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Date range to query.
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Optional Fund Plan filter.
    /// </summary>
    public FundPlanFilterModel? Filter { get; init; }

    /// <summary>
    /// Optional event ordering.
    /// </summary>
    public FundPlanBalanceEventSortModel? Sort { get; init; }
}