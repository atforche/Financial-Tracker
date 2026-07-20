using Models.FundPlans;

namespace Models.BalanceEvents;

/// <summary>
/// Query parameters for Fund Plan balance events in an Accounting Period range.
/// </summary>
public sealed class FundPlanBalanceEventsInAccountingPeriodRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Accounting Period range to query.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Optional Fund Plan filter.
    /// </summary>
    public FundPlanFilterModel? Filter { get; init; }

    /// <summary>
    /// Optional event ordering.
    /// </summary>
    public FundPlanBalanceEventSortModel? Sort { get; init; }
}