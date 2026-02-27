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
    /// Adds the provided pending debits to the current pending Account Balance
    /// </summary>
    internal AccountBalance AddNewPendingDebits(IEnumerable<FundAmount> pendingDebits) => new(Account, FundBalances, PendingDebits.Concat(pendingDebits), PendingCredits);

    /// <summary>
    /// Posts the provided pending debits to the current Account Balance
    /// </summary>
    internal AccountBalance PostPendingDebits(IEnumerable<FundAmount> pendingDebits)
    {
        IEnumerable<FundAmount> negativePendingDebits = pendingDebits.Select(fundAmount => new FundAmount
        {
            FundId = fundAmount.FundId,
            Amount = -fundAmount.Amount
        });
        if (Account.Type == AccountType.Debt)
        {
            return new AccountBalance(
                Account,
                FundBalances.Concat(pendingDebits),
                PendingDebits.Concat(negativePendingDebits),
                PendingCredits
            );
        }
        return new AccountBalance(
            Account,
            FundBalances.Concat(negativePendingDebits),
            PendingDebits.Concat(negativePendingDebits),
            PendingCredits
        );
    }

    /// <summary>
    /// Adds the provided pending credits to the current pending Account Balance
    /// </summary>
    internal AccountBalance AddNewPendingCredits(IEnumerable<FundAmount> pendingCredits) => new(Account, FundBalances, PendingDebits, PendingCredits.Concat(pendingCredits));

    /// <summary>
    /// Posts the provided pending credits to the current Account Balance
    /// </summary>
    internal AccountBalance PostPendingCredits(IEnumerable<FundAmount> pendingCredits)
    {
        IEnumerable<FundAmount> negativePendingCredits = pendingCredits.Select(fundAmount => new FundAmount
        {
            FundId = fundAmount.FundId,
            Amount = -fundAmount.Amount
        });
        if (Account.Type == AccountType.Debt)
        {
            return new AccountBalance(
                Account,
                FundBalances.Concat(negativePendingCredits),
                PendingDebits,
                PendingCredits.Concat(negativePendingCredits)
            );
        }
        return new AccountBalance(
            Account,
            FundBalances.Concat(pendingCredits),
            PendingDebits,
            PendingCredits.Concat(negativePendingCredits)
        );
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalance(Account account, IEnumerable<FundAmount> fundBalances, IEnumerable<FundAmount> pendingDebits, IEnumerable<FundAmount> pendingCredits)
    {
        Account = account;
        FundBalances = fundBalances
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        PendingDebits = pendingDebits
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        PendingCredits = pendingCredits
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
    }
}