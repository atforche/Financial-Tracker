using Domain.AccountingPeriods;

namespace Domain.Accounts.Queries;

/// <summary>
/// Represents Account balances and totals over an Accounting Period range.
/// </summary>
public sealed record AccountAccountingPeriodRange(
    QueryPage<AccountRangeBalance> Accounts,
    IReadOnlyCollection<string> AvailableAccountNames,
    decimal TotalIncome,
    decimal TrackedIncome,
    decimal TotalSpending,
    IReadOnlyCollection<AccountPeriodBalanceSummary> AccountingPeriods)
{
    /// <summary>
    /// Gets income deposited into untracked Accounts.
    /// </summary>
    public decimal UntrackedIncome => TotalIncome - TrackedIncome;
}

/// <summary>
/// Represents an Account's balances at the boundaries of a range.
/// </summary>
public sealed record AccountRangeBalance(Account Account, decimal StartingBalance, decimal EndingBalance);

/// <summary>
/// Represents Account balance summaries for an Accounting Period.
/// </summary>
public sealed record AccountPeriodBalanceSummary(
    AccountingPeriod AccountingPeriod,
    AccountBalanceSummary OpeningBalance,
    AccountBalanceSummary ClosingBalance);

/// <summary>
/// Represents an interpreted summary of Account balances.
/// </summary>
public sealed record AccountBalanceSummary(
    decimal TotalBalance,
    decimal TotalTrackedBalance,
    decimal TotalUntrackedBalance,
    IReadOnlyCollection<AccountTypeBalance> BalanceByAccountType);

/// <summary>
/// Represents a balance total for an Account type.
/// </summary>
public sealed record AccountTypeBalance(AccountType AccountType, decimal TotalBalance);
