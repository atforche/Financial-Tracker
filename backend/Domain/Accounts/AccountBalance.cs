using System.Diagnostics.CodeAnalysis;
using Domain.Funds;
using Domain.Transactions.Exceptions;

namespace Domain.Accounts;

/// <summary>
/// Value object class representing the balance of an Account
/// </summary>
public class AccountBalance
{
    /// <summary>
    /// Account for this Account Balance
    /// </summary>
    public Account Account { get; }

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
    /// Posted Balance for this Account Balance
    /// </summary>
    public decimal PostedBalance => FundBalances.Sum(balance => balance.Amount);

    /// <summary>
    /// Available to Spend Balance for this Account Balance
    /// </summary>
    public decimal? AvailableToSpend => Account.Type == AccountType.Debt ? null : PostedBalance - PendingDebits.Sum(debit => debit.Amount);

    /// <summary>
    /// Attempts to add the provided pending debits to the current pending Account Balance
    /// </summary>
    internal bool TryAddNewPendingDebits(IEnumerable<FundAmount> pendingDebits, [NotNullWhen(true)] out AccountBalance? newAccountBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(Account, FundBalances, PendingDebits.Concat(pendingDebits), PendingCredits, out newAccountBalance, out exceptions);

    /// <summary>
    /// Attempts to post the provided pending debits to the current Account Balance
    /// </summary>
    internal bool TryPostPendingDebits(IEnumerable<FundAmount> pendingDebits, [NotNullWhen(true)] out AccountBalance? newAccountBalance, out IEnumerable<Exception> exceptions)
    {
        IEnumerable<FundAmount> negativePendingDebits = pendingDebits.Select(fundAmount => new FundAmount
        {
            FundId = fundAmount.FundId,
            Amount = -fundAmount.Amount
        });
        if (Account.Type == AccountType.Debt)
        {
            return TryCreate(Account,
                FundBalances.Concat(pendingDebits),
                PendingDebits.Concat(negativePendingDebits),
                PendingCredits,
                out newAccountBalance,
                out exceptions);
        }
        return TryCreate(Account,
            FundBalances.Concat(negativePendingDebits),
            PendingDebits.Concat(negativePendingDebits),
            PendingCredits,
            out newAccountBalance,
            out exceptions);
    }

    /// <summary>
    /// Attempts to add the provided pending credits to the current pending Account Balance
    /// </summary>
    internal bool TryAddNewPendingCredits(IEnumerable<FundAmount> pendingCredits, [NotNullWhen(true)] out AccountBalance? newAccountBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(Account, FundBalances, PendingDebits, PendingCredits.Concat(pendingCredits), out newAccountBalance, out exceptions);

    /// <summary>
    /// Attempts to post the provided pending credits to the current Account Balance
    /// </summary>
    internal bool TryPostPendingCredits(IEnumerable<FundAmount> pendingCredits, [NotNullWhen(true)] out AccountBalance? newAccountBalance, out IEnumerable<Exception> exceptions)
    {
        IEnumerable<FundAmount> negativePendingCredits = pendingCredits.Select(fundAmount => new FundAmount
        {
            FundId = fundAmount.FundId,
            Amount = -fundAmount.Amount
        });
        if (Account.Type == AccountType.Debt)
        {
            return TryCreate(Account,
                FundBalances.Concat(negativePendingCredits),
                PendingDebits,
                PendingCredits.Concat(negativePendingCredits),
                out newAccountBalance,
                out exceptions);
        }
        return TryCreate(Account,
            FundBalances.Concat(pendingCredits),
            PendingDebits,
            PendingCredits.Concat(negativePendingCredits),
            out newAccountBalance,
            out exceptions);
    }

    /// <summary>
    /// Attempts to construct a new instance of this class
    /// </summary>
    internal static bool TryCreate(
        Account account,
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
        accountBalance = new AccountBalance(account, fundBalances, pendingDebits, pendingCredits);
        if (accountBalance.PostedBalance < 0 || accountBalance.AvailableToSpend < 0)
        {
            accountBalance = null;
            exceptions = exceptions.Append(new InvalidDebitAccountException("Account balance cannot be negative."));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalance(Account account, IEnumerable<FundAmount> fundBalances, IEnumerable<FundAmount> pendingDebits, IEnumerable<FundAmount> pendingCredits)
    {
        Account = account;
        FundBalances = fundBalances.ToList();
        PendingDebits = pendingDebits.ToList();
        PendingCredits = pendingCredits.ToList();
    }
}