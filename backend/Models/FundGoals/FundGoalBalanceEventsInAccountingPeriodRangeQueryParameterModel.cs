namespace Models.FundGoals;

/// <summary>
/// Query parameters for Fund Goal balance events in an Accounting Period range.
/// </summary>
public sealed class FundGoalBalanceEventsInAccountingPeriodRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Accounting Period range to query.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Optional Fund Goal filter.
    /// </summary>
    public FundGoalFilterModel? Filter { get; init; }

    /// <summary>
    /// Optional event ordering.
    /// </summary>
    public FundGoalBalanceEventSortModel? Sort { get; init; }
}
