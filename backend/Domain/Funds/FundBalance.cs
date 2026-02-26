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
    /// Posted Balance for this Fund Balance
    /// </summary>
    public decimal PostedBalance => AccountBalances.Sum(balance => balance.Amount);

    /// <summary>
    /// Adds the provided pending debit to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingDebit(AccountAmount pendingDebit) => new(FundId, AccountBalances, PendingDebits.Append(pendingDebit), PendingCredits);

    /// <summary>
    /// Posts the provided pending debit to the current posted Fund Balance
    /// </summary>
    internal FundBalance PostPendingDebit(AccountAmount pendingDebit)
    {
        var negativePendingDebit = new AccountAmount
        {
            AccountId = pendingDebit.AccountId,
            Amount = -pendingDebit.Amount
        };
        return new FundBalance(FundId, AccountBalances.Append(negativePendingDebit), PendingDebits.Append(negativePendingDebit), PendingCredits);
    }

    /// <summary>
    /// Adds the provided pending credit to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingCredit(AccountAmount pendingCredit) => new(FundId, AccountBalances, PendingDebits, PendingCredits.Append(pendingCredit));

    /// <summary>
    /// Posts the provided pending credit to the current posted Fund Balance
    /// </summary>
    internal FundBalance PostPendingCredit(AccountAmount pendingCredit)
    {
        var negativePendingCredit = new AccountAmount
        {
            AccountId = pendingCredit.AccountId,
            Amount = -pendingCredit.Amount
        };
        return new FundBalance(FundId, AccountBalances.Append(pendingCredit), PendingDebits, PendingCredits.Append(negativePendingCredit));
    }

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