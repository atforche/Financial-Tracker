using Domain.Transactions.Queries;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// An Accounting Period balance, its Transactions, and interpreted totals.
/// </summary>
public sealed record AccountingPeriodTransactions(
    AccountingPeriodBalance Balance,
    QueryPage<TransactionDetails> Transactions,
    decimal TotalIncome,
    decimal TrackedIncome,
    decimal TotalSpending)
{
    /// <summary>
    /// Gets income assigned to untracked Accounts.
    /// </summary>
    public decimal UntrackedIncome => TotalIncome - TrackedIncome;
}