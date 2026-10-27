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
    IReadOnlyCollection<AccountTypeBalance> BalanceByAccountType)
{
    /// <summary>
    /// Summarizes posted Account balances, counting debt Account balances against the totals.
    /// </summary>
    public static AccountBalanceSummary Create(IEnumerable<(AccountType Type, decimal Balance)> balances)
    {
        var values = balances.Select(balance => (
            balance.Type,
            Amount: balance.Type.IsDebt() ? -balance.Balance : balance.Balance)).ToList();
        return new AccountBalanceSummary(
            values.Sum(item => item.Amount),
            values.Where(item => item.Type.IsTracked()).Sum(item => item.Amount),
            values.Where(item => !item.Type.IsTracked()).Sum(item => item.Amount),
            values.GroupBy(item => item.Type).OrderBy(group => group.Key)
                .Select(group => new AccountTypeBalance(group.Key, group.Sum(item => item.Amount))).ToList());
    }
}

/// <summary>
/// Represents a balance total for an Account type.
/// </summary>
public sealed record AccountTypeBalance(AccountType AccountType, decimal TotalBalance);
