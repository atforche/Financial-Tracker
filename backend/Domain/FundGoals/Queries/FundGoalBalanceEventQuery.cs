namespace Domain.FundGoals.Queries;

/// <summary>
/// Criteria for querying Fund Goal balance events over an Accounting Period range.
/// </summary>
public sealed record FundGoalBalanceEventAccountingPeriodRangeQuery(
    Guid StartId,
    Guid EndId,
    FundGoalBalanceEventFilter Filter);

/// <summary>
/// Criteria used to filter Fund Goal balance events.
/// </summary>
public sealed record FundGoalBalanceEventFilter(IReadOnlyCollection<Guid> FundIds);
