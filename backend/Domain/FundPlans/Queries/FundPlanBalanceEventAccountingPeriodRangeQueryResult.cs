namespace Domain.FundPlans.Queries;

/// <summary>
/// Result of querying Fund Plan balance events over an Accounting Period range.
/// </summary>
public sealed record FundPlanBalanceEventAccountingPeriodRangeQueryResult(QueryPage<FundPlanBalanceEvent>? Page);