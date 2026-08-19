namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents a contiguous range of Accounting Periods and its interpreted totals.
/// </summary>
public sealed record AccountingPeriodRange(
    QueryPage<AccountingPeriodBalance> AccountingPeriods,
    decimal TotalIncome,
    decimal TrackedIncome,
    decimal TotalSpending)
{
    /// <summary>
    /// Gets income deposited into untracked Accounts.
    /// </summary>
    public decimal UntrackedIncome => TotalIncome - TrackedIncome;
}
