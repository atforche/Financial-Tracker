namespace Domain.FundGoals.Queries;

/// <summary>
/// Criteria for querying Fund Goal balance events over a date range.
/// </summary>
public sealed record FundGoalBalanceEventQuery(
    DateOnly Start,
    DateOnly End,
    FundGoalBalanceEventFilter Filter,
    FundGoalBalanceEventSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Criteria for querying Fund Goal balance events over an Accounting Period range.
/// </summary>
public sealed record FundGoalBalanceEventAccountingPeriodRangeQuery(
    Guid StartId,
    Guid EndId,
    FundGoalBalanceEventFilter Filter,
    FundGoalBalanceEventSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Criteria used to filter Fund Goal balance events.
/// </summary>
public sealed record FundGoalBalanceEventFilter(IReadOnlyCollection<Guid> FundIds);

/// <summary>
/// Supported Fund Goal balance-event sort orders.
/// </summary>
public enum FundGoalBalanceEventSort
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

    /// <summary>
    /// Sorts by the other party for the balance-event direction ascending.
    /// </summary>
    Counterparty,

    /// <summary>
    /// Sorts by the other party for the balance-event direction descending.
    /// </summary>
    CounterpartyDescending,

    /// <summary>
    /// Sorts by source ascending.
    /// </summary>
    Source,

    /// <summary>
    /// Sorts by source descending.
    /// </summary>
    SourceDescending,

    /// <summary>
    /// Sorts by destination ascending.
    /// </summary>
    Destination,

    /// <summary>
    /// Sorts by destination descending.
    /// </summary>
    DestinationDescending,
}