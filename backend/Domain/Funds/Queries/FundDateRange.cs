namespace Domain.Funds.Queries;

/// <summary>
/// Represents Fund balances and totals over a date range.
/// </summary>
public sealed record FundDateRange(
    QueryPage<FundRangeBalance> Funds,
    IReadOnlyCollection<string> AvailableFundNames,
    decimal TotalIncome,
    decimal TrackedIncome,
    decimal TotalSpending,
    IReadOnlyCollection<FundDateBalanceSummary> Dates)
{
    /// <summary>
    /// Gets income deposited into untracked Accounts.
    /// </summary>
    public decimal UntrackedIncome => TotalIncome - TrackedIncome;
}

/// <summary>
/// Represents a Fund balance summary for a date.
/// </summary>
public sealed record FundDateBalanceSummary(DateOnly Date, FundBalanceSummary Balance);
