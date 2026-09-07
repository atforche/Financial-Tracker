namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents a contiguous range of Accounting Periods and its interpreted totals.
/// </summary>
public sealed record AccountingPeriodRange(
    QueryPage<AccountingPeriodBalance> AccountingPeriods,
    decimal TotalIncome,
    decimal TrackedIncome,
    decimal TotalSpending,
    decimal TotalExpectedIncome,
    decimal TrackedExpectedIncome)
{
    /// <summary>
    /// Gets income deposited into untracked Accounts.
    /// </summary>
    public decimal UntrackedIncome => TotalIncome - TrackedIncome;

    /// <summary>
    /// Gets expected income assigned to untracked Accounts.
    /// </summary>
    public decimal UntrackedExpectedIncome => TotalExpectedIncome - TrackedExpectedIncome;
}
