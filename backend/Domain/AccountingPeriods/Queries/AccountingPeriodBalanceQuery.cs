namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Criteria for querying Accounting Periods and their balances.
/// </summary>
public sealed record AccountingPeriodBalanceQuery(
    AccountingPeriodFilter Filter,
    int Offset,
    int? Limit);
