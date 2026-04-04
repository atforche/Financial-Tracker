using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Value object class representing an Account involved in a Transaction
/// </summary>
public class TransactionAccount
{
    private readonly List<FundAmount> _fundAmounts = [];

    /// <summary>
    /// Parent Transaction for this Transaction Account
    /// </summary>
    public Transaction Transaction { get; private set; }

    /// <summary>
    /// Account ID for this Transaction Account
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// Type for this Transaction Account
    /// </summary>
    public TransactionAccountType Type { get; private set; }

    /// <summary>
    /// Fund Amounts for this Transaction Account
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAmounts => _fundAmounts;

    /// <summary>
    /// Date that the Transaction was posted to this Account
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Applies this Transaction Account to the provided existing Account Balance as of the specified date
    /// </summary>
    internal AccountBalance ApplyToAccountBalance(AccountBalance existingAccountBalance, DateOnly date) =>
        ApplyFundAmountsToAccountBalance(existingAccountBalance, date, FundAmounts);

    /// <summary>
    /// Reverses this Transaction Account from the provided existing Account Balance as of the specified date
    /// </summary>
    internal AccountBalance ReverseFromAccountBalance(AccountBalance existingAccountBalance, DateOnly date) =>
        ApplyFundAmountsToAccountBalance(existingAccountBalance, date, FundAmounts.Select(f => new FundAmount
        {
            FundId = f.FundId,
            Amount = -f.Amount
        }).ToList());

    /// <summary>
    /// Applies this Transaction Account to the provided existing Fund Balance as of the specified date
    /// </summary>
    internal FundBalance ApplyToFundBalance(FundBalance existingFundBalance, DateOnly date) =>
        ApplyFundAmountsToFundBalance(existingFundBalance, date, FundAmounts);

    /// <summary>
    /// Reverses this Transaction Account from the provided existing Fund Balance as of the specified date
    /// </summary>
    internal FundBalance ReverseFromFundBalance(FundBalance existingFundBalance, DateOnly date) =>
        ApplyFundAmountsToFundBalance(existingFundBalance, date, FundAmounts.Select(f => new FundAmount
        {
            FundId = f.FundId,
            Amount = -f.Amount
        }).ToList());

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal TransactionAccount(
        Transaction transaction,
        AccountId accountId,
        TransactionAccountType type,
        IEnumerable<FundAmount> fundAmounts)
    {
        Transaction = transaction;
        AccountId = accountId;
        Type = type;
        _fundAmounts = fundAmounts.ToList();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private TransactionAccount()
    {
        Transaction = null!;
        AccountId = null!;
    }

    /// <summary>
    /// Applies the provided Fund Amounts to the provided existing Account Balance as of the specified date
    /// </summary>
    private AccountBalance ApplyFundAmountsToAccountBalance(AccountBalance existingAccountBalance, DateOnly date, IReadOnlyCollection<FundAmount> fundAmounts)
    {
        if (existingAccountBalance.Account.Id != AccountId)
        {
            return existingAccountBalance;
        }
        AccountBalance newAccountBalance = existingAccountBalance;
        if (Transaction.Date == date)
        {
            newAccountBalance = Type == TransactionAccountType.Debit
                ? newAccountBalance.AddNewPendingDebits(fundAmounts)
                : newAccountBalance.AddNewPendingCredits(fundAmounts);
        }
        if (PostedDate == date)
        {
            newAccountBalance = Type == TransactionAccountType.Debit
                ? newAccountBalance.PostPendingDebits(fundAmounts)
                : newAccountBalance.PostPendingCredits(fundAmounts);
        }
        return newAccountBalance;
    }

    /// <summary>
    /// Applies the provided Fund Amounts to the provided existing Fund Balance as of the specified date
    /// </summary>
    private FundBalance ApplyFundAmountsToFundBalance(FundBalance existingFundBalance, DateOnly date, IReadOnlyCollection<FundAmount> fundAmounts)
    {
        FundAmount? fundAmount = fundAmounts.SingleOrDefault(f => f.FundId == existingFundBalance.FundId);
        if (fundAmount == null)
        {
            return existingFundBalance;
        }
        var accountAmount = new AccountAmount
        {
            AccountId = AccountId,
            Amount = fundAmount.Amount
        };
        FundBalance newFundBalance = existingFundBalance;
        if (Transaction.Date == date)
        {
            newFundBalance = Type == TransactionAccountType.Debit
                ? newFundBalance.AddNewPendingDebit(accountAmount)
                : newFundBalance.AddNewPendingCredit(accountAmount);
        }
        if (PostedDate == date)
        {
            newFundBalance = Type == TransactionAccountType.Debit
                ? newFundBalance.PostPendingDebit(accountAmount)
                : newFundBalance.PostPendingCredit(accountAmount);
        }
        return newFundBalance;
    }
}