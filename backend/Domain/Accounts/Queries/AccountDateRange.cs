namespace Domain.Accounts.Queries;

/// <summary>
/// Represents Account balances and totals over a date range.
/// </summary>
public sealed record AccountDateRange(
    QueryPage<AccountRangeBalance> Accounts,
    IReadOnlyCollection<string> AvailableAccountNames,
    AccountBalanceSummary? OpeningBalance,
    BalanceGranularity Granularity,
    IReadOnlyCollection<AccountDateBalanceSummary> Dates);

/// <summary>
/// Represents an Account balance summary for a date.
/// </summary>
public sealed record AccountDateBalanceSummary(DateOnly Date, AccountBalanceSummary Balance);
