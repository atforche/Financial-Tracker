using Domain.AccountingPeriods.Queries;

namespace Domain.Funds.Queries;

/// <summary>
/// Result of querying Fund balance events over an Accounting Period range.
/// </summary>
public sealed record FundBalanceEventAccountingPeriodRangeQueryResult(
    QueryPage<FundBalanceEvent>? Page,
    AccountingPeriodRangeQueryFailure Failure);