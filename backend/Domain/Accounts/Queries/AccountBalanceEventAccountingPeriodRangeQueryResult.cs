namespace Domain.Accounts.Queries;

/// <summary>
/// Result of querying Account balance events over an Accounting Period range.
/// </summary>
public sealed record AccountBalanceEventAccountingPeriodRangeQueryResult(QueryPage<AccountBalanceEvent>? Page);