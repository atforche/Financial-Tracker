using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;

namespace Domain.Funds;

/// <summary>
/// Value object class representing the balance of a Fund
/// </summary>
public class FundBalance
{
    /// <summary>
    /// Fund for this Fund Balance
    /// </summary>
    public FundId FundId { get; }

    /// <summary>
    /// Account Balances for this Fund Balance
    /// </summary>
    public IReadOnlyCollection<AccountAmount> AccountBalances { get; }

    /// <summary>
    /// Pending Debits for this Fund Balance
    /// </summary>
    public IReadOnlyCollection<AccountAmount> PendingDebits { get; }

    /// <summary>
    /// Pending Credits for this Fund Balance
    /// </summary>
    public IReadOnlyCollection<AccountAmount> PendingCredits { get; }

    /// <summary>
    /// Balance for this Fund Balance
    /// </summary>
    public decimal Balance => AccountBalances.Sum(balance => balance.Amount);

    /// <summary>
    /// Attempts to construct a new instance of this class
    /// </summary>
    internal static bool TryCreate(
        FundId fundId,
        IEnumerable<AccountAmount> accountBalances,
        IEnumerable<AccountAmount> pendingDebits,
        IEnumerable<AccountAmount> pendingCredits,
        out FundBalance? fundBalance,
        out IEnumerable<Exception> exceptions)
    {
        fundBalance = null;
        exceptions = [];

        accountBalances = accountBalances
            .GroupBy(accountAmount => accountAmount.AccountId)
            .Select(group => new AccountAmount
            {
                AccountId = group.Key,
                Amount = group.Sum(accountAmount => accountAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        pendingDebits = pendingDebits
            .GroupBy(accountAmount => accountAmount.AccountId)
            .Select(group => new AccountAmount
            {
                AccountId = group.Key,
                Amount = group.Sum(accountAmount => accountAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        pendingCredits = pendingCredits
            .GroupBy(accountAmount => accountAmount.AccountId)
            .Select(group => new AccountAmount
            {
                AccountId = group.Key,
                Amount = group.Sum(accountAmount => accountAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        fundBalance = new FundBalance(fundId, accountBalances, pendingDebits, pendingCredits);
        return true;
    }

    /// <summary>
    /// Attempts to add the provided Account Amount to the current Fund Balance
    /// </summary>
    internal bool TryAddNewAmount(AccountAmount accountAmount, [NotNullWhen(true)] out FundBalance? newFundBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(FundId, AccountBalances.Append(accountAmount), PendingDebits, PendingCredits, out newFundBalance, out exceptions);

    /// <summary>
    /// Attempts to add the provided pending debits to the current pending Fund Balance
    /// </summary>
    internal bool TryAddNewPendingDebits(AccountAmount accountAmount, [NotNullWhen(true)] out FundBalance? newFundBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(FundId, AccountBalances, PendingDebits.Append(accountAmount), PendingCredits, out newFundBalance, out exceptions);

    /// <summary>
    /// Attempts to add the provided pending credits to the current pending Fund Balance
    /// </summary>
    internal bool TryAddNewPendingCredits(AccountAmount accountAmount, [NotNullWhen(true)] out FundBalance? newFundBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(FundId, AccountBalances, PendingDebits, PendingCredits.Append(accountAmount), out newFundBalance, out exceptions);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundBalance(FundId fundId, IEnumerable<AccountAmount> accountBalances, IEnumerable<AccountAmount> pendingDebits, IEnumerable<AccountAmount> pendingCredits)
    {
        FundId = fundId;
        AccountBalances = accountBalances.ToList();
        PendingDebits = pendingDebits.ToList();
        PendingCredits = pendingCredits.ToList();
    }
}