using Domain.Accounts;
using Domain.Budgets;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Value object class representing an Account involved in a Transaction
/// </summary>
public class TransactionAccount
{
    private readonly List<FundAmount> _fundAmounts = [];
    private readonly List<BudgetAmount> _budgetAmounts = [];

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
    /// Budget Amounts for this Transaction Account
    /// </summary>
    public IReadOnlyCollection<BudgetAmount> BudgetAmounts => _budgetAmounts;

    /// <summary>
    /// Date that the Transaction was posted to this Account
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Applies this Transaction Account to the provided existing Account Balance as of the specified date
    /// </summary>
    internal AccountBalance ApplyToAccountBalance(AccountBalance existingAccountBalance, DateOnly date) =>
        ApplyFundAmountsToAccountBalance(existingAccountBalance, date, FundAmounts.Concat(BudgetAmounts.Select(b => new FundAmount
        {
            FundId = b.Budget.FundId,
            Amount = b.Amount
        })).ToList());

    /// <summary>
    /// Reverses this Transaction Account from the provided existing Account Balance as of the specified date
    /// </summary>
    internal AccountBalance ReverseFromAccountBalance(AccountBalance existingAccountBalance, DateOnly date) =>
        ApplyFundAmountsToAccountBalance(existingAccountBalance, date, FundAmounts.Select(f => new FundAmount
        {
            FundId = f.FundId,
            Amount = -f.Amount
        }).Concat(BudgetAmounts.Select(b => new FundAmount
        {
            FundId = b.Budget.FundId,
            Amount = -b.Amount
        })).ToList());

    /// <summary>
    /// Applies this Transaction Account to the provided existing Fund Balance as of the specified date
    /// </summary>
    internal FundBalance ApplyToFundBalance(FundBalance existingFundBalance, DateOnly date) =>
        ApplyFundAmountsToFundBalance(existingFundBalance, date, FundAmounts, BudgetAmounts);

    /// <summary>
    /// Reverses this Transaction Account from the provided existing Fund Balance as of the specified date
    /// </summary>
    internal FundBalance ReverseFromFundBalance(FundBalance existingFundBalance, DateOnly date) =>
        ApplyFundAmountsToFundBalance(existingFundBalance, date,
            FundAmounts.Select(f => new FundAmount
            {
                FundId = f.FundId,
                Amount = -f.Amount
            }).ToList(),
            BudgetAmounts.Select(b => new BudgetAmount
            {
                Budget = b.Budget,
                Amount = -b.Amount
            }).ToList());

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal TransactionAccount(
        Transaction transaction,
        AccountId accountId,
        TransactionAccountType type,
        IEnumerable<FundAmount> fundAmounts,
        IEnumerable<BudgetAmount> budgetAmounts)
    {
        Transaction = transaction;
        AccountId = accountId;
        Type = type;
        _fundAmounts = fundAmounts.ToList();
        _budgetAmounts = budgetAmounts.ToList();
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
    /// Applies the provided Fund Amounts and Budget Amounts to the provided existing Fund Balance as of the specified date
    /// </summary>
    private FundBalance ApplyFundAmountsToFundBalance(FundBalance existingFundBalance, DateOnly date,
        IReadOnlyCollection<FundAmount> fundAmounts, IReadOnlyCollection<BudgetAmount> budgetAmounts)
    {
        decimal totalAmount = fundAmounts
            .Where(f => f.FundId == existingFundBalance.FundId)
            .Sum(f => f.Amount);
        totalAmount += budgetAmounts
            .Where(b => b.Budget.FundId == existingFundBalance.FundId)
            .Sum(b => b.Amount);
        if (totalAmount == 0)
        {
            return existingFundBalance;
        }
        var accountAmount = new AccountAmount
        {
            AccountId = AccountId,
            Amount = totalAmount
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