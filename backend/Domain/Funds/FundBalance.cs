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
    /// Attempts to add the provided Account Amount to the current Fund Balance
    /// </summary>
    internal FundBalance AddNewAmount(AccountAmount accountAmount) => new(FundId, AccountBalances.Append(accountAmount), PendingDebits, PendingCredits);

    /// <summary>
    /// Attempts to add the provided pending debits to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingDebits(AccountAmount accountAmount) => new(FundId, AccountBalances, PendingDebits.Append(accountAmount), PendingCredits);

    /// <summary>
    /// Attempts to add the provided pending credits to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingCredits(AccountAmount accountAmount) => new(FundId, AccountBalances, PendingDebits, PendingCredits.Append(accountAmount));

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundBalance(FundId fundId, IEnumerable<AccountAmount> accountBalances, IEnumerable<AccountAmount> pendingDebits, IEnumerable<AccountAmount> pendingCredits)
    {
        FundId = fundId;
        AccountBalances = accountBalances
            .GroupBy(accountAmount => accountAmount.AccountId)
            .Select(group => new AccountAmount
            {
                AccountId = group.Key,
                Amount = group.Sum(accountAmount => accountAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        PendingDebits = pendingDebits
            .GroupBy(accountAmount => accountAmount.AccountId)
            .Select(group => new AccountAmount
            {
                AccountId = group.Key,
                Amount = group.Sum(accountAmount => accountAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        PendingCredits = pendingCredits
            .GroupBy(accountAmount => accountAmount.AccountId)
            .Select(group => new AccountAmount
            {
                AccountId = group.Key,
                Amount = group.Sum(accountAmount => accountAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
    }
}