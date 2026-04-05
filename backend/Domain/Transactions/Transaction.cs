using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Budgets;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a Transaction
/// </summary>
/// <remarks>
/// A Transaction represents a financial transaction were money is moved between accounts or funds.
/// </remarks>
public class Transaction : Entity<TransactionId>
{
    /// <summary>
    /// Accounting Period for this Transaction
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <summary>
    /// Date for this Transaction
    /// </summary>
    public DateOnly Date { get; internal set; }

    /// <summary>
    /// Sequence number for this Transaction
    /// </summary> 
    /// <remarks>
    /// The sequence number is used to order multiple transactions for the same date.
    /// </remarks>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Location for this Transaction
    /// </summary>
    public string Location { get; internal set; }

    /// <summary>
    /// Description for this Transaction
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Amount for this Transaction
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Debit Account for this Transaction
    /// </summary>
    public TransactionAccount? DebitAccount
    {
        get;
        internal set
        {
            field = value;
            if (value != null)
            {
                Amount = value.FundAmounts.Sum(fundAmount => fundAmount.Amount) + value.BudgetAmounts.Sum(budgetAmount => budgetAmount.Amount);
            }
        }
    }

    /// <summary>
    /// Credit Account for this Transaction
    /// </summary>
    public TransactionAccount? CreditAccount
    {
        get;
        internal set
        {
            field = value;
            if (value != null)
            {
                Amount = value.FundAmounts.Sum(fundAmount => fundAmount.Amount) + value.BudgetAmounts.Sum(budgetAmount => budgetAmount.Amount);
            }
        }
    }

    /// <summary>
    /// Account ID of the Account that generated this transaction when it was created, or null
    /// </summary>
    public AccountId? GeneratedByAccountId { get; internal set; }

    /// <summary>
    /// Applies this Transaction to the provided existing Account Balance as of the specified date
    /// </summary>
    internal AccountBalance ApplyToAccountBalance(AccountBalance existingAccountBalance, DateOnly date)
    {
        AccountBalance newAccountBalance = existingAccountBalance;
        newAccountBalance = DebitAccount != null ? DebitAccount.ApplyToAccountBalance(newAccountBalance, date) : newAccountBalance;
        newAccountBalance = CreditAccount != null ? CreditAccount.ApplyToAccountBalance(newAccountBalance, date) : newAccountBalance;
        return newAccountBalance;
    }

    /// <summary>
    /// Applies this Transaction to the provided existing Fund Balance as of the specified date
    /// </summary>
    internal FundBalance ApplyToFundBalance(FundBalance existingFundBalance, DateOnly date)
    {
        FundBalance newFundBalance = existingFundBalance;
        newFundBalance = DebitAccount != null ? DebitAccount.ApplyToFundBalance(newFundBalance, date) : newFundBalance;
        newFundBalance = CreditAccount != null ? CreditAccount.ApplyToFundBalance(newFundBalance, date) : newFundBalance;
        return newFundBalance;
    }

    /// <summary>
    /// Applies this Transaction to the provided existing Budget Balance as of the specified date
    /// </summary>
    internal BudgetBalance ApplyToBudgetBalance(BudgetBalance existingBudgetBalance, DateOnly date)
    {
        BudgetBalance newBudgetBalance = existingBudgetBalance;
        newBudgetBalance = DebitAccount != null ? DebitAccount.ApplyToBudgetBalance(newBudgetBalance, date) : newBudgetBalance;
        newBudgetBalance = CreditAccount != null ? CreditAccount.ApplyToBudgetBalance(newBudgetBalance, date) : newBudgetBalance;
        return newBudgetBalance;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Transaction(CreateTransactionRequest request, int sequence)
        : base(new TransactionId(Guid.NewGuid()))
    {
        AccountingPeriodId = request.AccountingPeriod.Id;
        Date = request.Date;
        Sequence = sequence;
        Location = request.Location;
        Description = request.Description;
        DebitAccount = request.DebitAccount != null
            ? new TransactionAccount(this, request.DebitAccount.Account.Id, TransactionAccountType.Debit,
                request.DebitAccount.FundAmounts, request.DebitAccount.BudgetAmounts)
            : null;
        CreditAccount = request.CreditAccount != null
            ? new TransactionAccount(this, request.CreditAccount.Account.Id, TransactionAccountType.Credit,
                request.CreditAccount.FundAmounts, request.CreditAccount.BudgetAmounts)
            : null;
        GeneratedByAccountId = request.IsInitialTransactionForAccount ? CreditAccount?.AccountId : null;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Transaction() : base()
    {
        AccountingPeriodId = null!;
        Location = null!;
        Description = null!;
        DebitAccount = null;
        CreditAccount = null;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="Transaction"/>
/// </summary>
public record TransactionId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal TransactionId(Guid value)
        : base(value)
    {
    }
}