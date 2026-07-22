namespace Domain.FundPlans.Queries;

/// <summary>
/// Criteria for querying Fund Plan balance events over a date range.
/// </summary>
public sealed record FundPlanBalanceEventQuery(
    DateOnly Start,
    DateOnly End,
    FundPlanBalanceEventFilter Filter,
    FundPlanBalanceEventSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Criteria for querying Fund Plan balance events over an Accounting Period range.
/// </summary>
public sealed record FundPlanBalanceEventAccountingPeriodRangeQuery(
    Guid StartId,
    Guid EndId,
    FundPlanBalanceEventFilter Filter,
    FundPlanBalanceEventSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Criteria used to filter Fund Plan balance events.
/// </summary>
public sealed record FundPlanBalanceEventFilter(IReadOnlyCollection<Guid> FundIds);

/// <summary>
/// Supported Fund Plan balance-event sort orders.
/// </summary>
public enum FundPlanBalanceEventSort
{
    /// <summary>
    /// Sorts by Fund name ascending.
    /// </summary>
    FundName,

    /// <summary>
    /// Sorts by Fund name descending.
    /// </summary>
    FundNameDescending,

    /// <summary>
    /// Sorts by date ascending.
    /// </summary>
    Date,

    /// <summary>
    /// Sorts by date descending.
    /// </summary>
    DateDescending,

    /// <summary>
    /// Sorts by event type ascending.
    /// </summary>
    Type,

    /// <summary>
    /// Sorts by event type descending.
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Sorts by amount ascending.
    /// </summary>
    Amount,

    /// <summary>
    /// Sorts by amount descending.
    /// </summary>
    AmountDescending,
}