namespace Domain.Accounts.Queries;

/// <summary>
/// Represents Account balances and totals over a date range.
/// </summary>
public sealed record AccountDateRange(
    QueryPage<AccountRangeBalance> Accounts,
    IReadOnlyCollection<string> AvailableAccountNames,
    decimal TotalIncome,
    decimal TrackedIncome,
    decimal TotalSpending,
    IReadOnlyCollection<AccountDateBalanceSummary> Dates)
{
    /// <summary>
    /// Gets income deposited into untracked Accounts.
    /// </summary>
    public decimal UntrackedIncome => TotalIncome - TrackedIncome;
}

/// <summary>
/// Represents an Account balance summary for a date.
/// </summary>
public sealed record AccountDateBalanceSummary(DateOnly Date, AccountBalanceSummary Balance);
