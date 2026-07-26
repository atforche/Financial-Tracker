using Domain.AccountingPeriods.Queries;

namespace Domain.FundGoals.Queries;

/// <summary>
/// Result of querying Fund Goal balance events over an Accounting Period range.
/// </summary>
public sealed record FundGoalBalanceEventAccountingPeriodRangeQueryResult(
    QueryPage<FundGoalBalanceEvent>? Page,
    AccountingPeriodRangeQueryFailure Failure);