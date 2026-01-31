using System.Diagnostics.CodeAnalysis;
using Domain.Funds;

namespace Domain.Accounts;

/// <summary>
/// Value object class representing the balance of an Account
/// </summary>
public class AccountBalance
{
    /// <summary>
    /// Account for this Account Balance
    /// </summary>
    public AccountId AccountId { get; }

    /// <summary>
    /// Fund Balances for this Account Balance
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundBalances { get; }

    /// <summary>
    /// Pending Debits for this Account Balance
    /// </summary>
    public IReadOnlyCollection<FundAmount> PendingDebits { get; }

    /// <summary>
    /// Pending Credits for this Account Balance
    /// </summary>
    public IReadOnlyCollection<FundAmount> PendingCredits { get; }

    /// <summary>
    /// Balance for this Account Balance
    /// </summary>
    public decimal Balance => FundBalances.Sum(balance => balance.Amount);

    /// <summary>
    /// Attempts to construct a new instance of this class
    /// </summary>
    internal static bool TryCreate(
        AccountId accountId,
        IEnumerable<FundAmount> fundBalances,
        IEnumerable<FundAmount> pendingDebits,
        IEnumerable<FundAmount> pendingCredits,
        out AccountBalance? accountBalance,
        out IEnumerable<Exception> exceptions)
    {
        accountBalance = null;
        exceptions = [];

        fundBalances = fundBalances
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        pendingDebits = pendingDebits
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        pendingCredits = pendingCredits
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        if (fundBalances.Sum(balance => balance.Amount) < 0 ||
            fundBalances.Sum(balance => balance.Amount) - pendingDebits.Sum(balance => balance.Amount) < 0)
        {
            exceptions = exceptions.Append(new InvalidOperationException("Account balance cannot be negative."));
            return false;
        }
        accountBalance = new AccountBalance(accountId, fundBalances, pendingDebits, pendingCredits);
        return true;
    }

    /// <summary>
    /// Attempts to add the provided Fund Amounts to the current Account Balance
    /// </summary>
    internal bool TryAddNewAmounts(IEnumerable<FundAmount> fundAmounts, [NotNullWhen(true)] out AccountBalance? newAccountBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(AccountId, FundBalances.Concat(fundAmounts), PendingDebits, PendingCredits, out newAccountBalance, out exceptions);

    /// <summary>
    /// Attempts to add the provided pending debits to the current pending Account Balance
    /// </summary>
    internal bool TryAddNewPendingDebits(IEnumerable<FundAmount> pendingDebits, [NotNullWhen(true)] out AccountBalance? newAccountBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(AccountId, FundBalances, PendingDebits.Concat(pendingDebits), PendingCredits, out newAccountBalance, out exceptions);

    /// <summary>
    /// Attempts to add the provided pending credits to the current pending Account Balance
    /// </summary>
    internal bool TryAddNewPendingCredits(IEnumerable<FundAmount> pendingCredits, [NotNullWhen(true)] out AccountBalance? newAccountBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(AccountId, FundBalances, PendingDebits, PendingCredits.Concat(pendingCredits), out newAccountBalance, out exceptions);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalance(AccountId accountId, IEnumerable<FundAmount> fundBalances, IEnumerable<FundAmount> pendingDebits, IEnumerable<FundAmount> pendingCredits)
    {
        AccountId = accountId;
        FundBalances = fundBalances.ToList();
        PendingDebits = pendingDebits.ToList();
        PendingCredits = pendingCredits.ToList();
    }
}