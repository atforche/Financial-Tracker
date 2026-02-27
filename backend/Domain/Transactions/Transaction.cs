using Domain.AccountingPeriods;
using Domain.Accounts;
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
    public decimal Amount => DebitAccount?.FundAmounts.Sum(fundAmount => fundAmount.Amount) ?? CreditAccount?.FundAmounts.Sum(fundAmount => fundAmount.Amount) ?? 0m;

    /// <summary>
    /// Debit Account for this Transaction
    /// </summary>
    public TransactionAccount? DebitAccount { get; internal set; }

    /// <summary>
    /// Credit Account for this Transaction
    /// </summary>
    public TransactionAccount? CreditAccount { get; internal set; }

    /// <summary>
    /// Account ID of the Account that generated this transaction when it was created, or null
    /// </summary>
    public AccountId? GeneratedByAccountId { get; internal set; }

    /// <summary>
    /// Applies this Transaction to the provided existing Account Balance as of the specified date
    /// </summary>
    public AccountBalance ApplyToAccountBalance(AccountBalance existingAccountBalance, DateOnly date)
    {
        AccountBalance newAccountBalance = existingAccountBalance;
        if (DebitAccount?.AccountId == existingAccountBalance.Account.Id)
        {
            newAccountBalance = ApplyToAccountBalancePrivate(DebitAccount, TransactionAccountType.Debit, newAccountBalance, date);
        }
        if (CreditAccount?.AccountId == existingAccountBalance.Account.Id)
        {
            newAccountBalance = ApplyToAccountBalancePrivate(CreditAccount, TransactionAccountType.Credit, newAccountBalance, date);
        }
        return newAccountBalance;
    }

    /// <summary>
    /// Applies this Transaction to the provided existing Fund Balance as of the specified date
    /// </summary>
    public FundBalance ApplyToFundBalance(FundBalance existingFundBalance, DateOnly date)
    {
        FundBalance newFundBalance = existingFundBalance;
        FundAmount? debitFundAmount = DebitAccount?.FundAmounts.SingleOrDefault(fundAmount => fundAmount.FundId == existingFundBalance.FundId);
        if (DebitAccount != null && debitFundAmount != null)
        {
            newFundBalance = ApplyToFundBalancePrivate(DebitAccount, debitFundAmount, TransactionAccountType.Debit, newFundBalance, date);
        }
        FundAmount? creditFundAmount = CreditAccount?.FundAmounts.SingleOrDefault(fundAmount => fundAmount.FundId == existingFundBalance.FundId);
        if (CreditAccount != null && creditFundAmount != null)
        {
            newFundBalance = ApplyToFundBalancePrivate(CreditAccount, creditFundAmount, TransactionAccountType.Credit, newFundBalance, date);
        }
        return newFundBalance;
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
        DebitAccount = request.DebitAccount != null ? new TransactionAccount(this, request.DebitAccount.Account.Id, request.DebitAccount.FundAmounts) : null;
        CreditAccount = request.CreditAccount != null ? new TransactionAccount(this, request.CreditAccount.Account.Id, request.CreditAccount.FundAmounts) : null;
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

    /// <summary>
    /// Applies this Transaction to the provided existing Account Balance as of the specified date
    /// </summary>
    private AccountBalance ApplyToAccountBalancePrivate(
        TransactionAccount transactionAccount,
        TransactionAccountType transactionAccountType,
        AccountBalance existingAccountBalance,
        DateOnly date)
    {
        AccountBalance newAccountBalance = existingAccountBalance;
        if (Date == date)
        {
            newAccountBalance = transactionAccountType == TransactionAccountType.Debit
                ? newAccountBalance.AddNewPendingDebits(transactionAccount.FundAmounts)
                : newAccountBalance.AddNewPendingCredits(transactionAccount.FundAmounts);
        }
        if (transactionAccount.PostedDate == date)
        {
            newAccountBalance = transactionAccountType == TransactionAccountType.Debit
                ? newAccountBalance.PostPendingDebits(transactionAccount.FundAmounts)
                : newAccountBalance.PostPendingCredits(transactionAccount.FundAmounts);
        }
        return newAccountBalance;
    }

    /// <summary>
    /// Applies this Transaction to the provided existing Fund Balance as of the specified date
    /// </summary>
    private FundBalance ApplyToFundBalancePrivate(
        TransactionAccount transactionAccount,
        FundAmount fundAmount,
        TransactionAccountType transactionAccountType,
        FundBalance existingFundBalance,
        DateOnly date)
    {
        var accountAmount = new AccountAmount
        {
            AccountId = transactionAccount.AccountId,
            Amount = fundAmount.Amount
        };
        FundBalance newFundBalance = existingFundBalance;
        if (Date == date)
        {
            newFundBalance = transactionAccountType == TransactionAccountType.Debit
                ? newFundBalance.AddNewPendingDebit(accountAmount)
                : newFundBalance.AddNewPendingCredit(accountAmount);
        }
        if (transactionAccount.PostedDate == date)
        {
            newFundBalance = transactionAccountType == TransactionAccountType.Debit
                ? newFundBalance.PostPendingDebit(accountAmount)
                : newFundBalance.PostPendingCredit(accountAmount);
        }
        return newFundBalance;
    }

    /// <summary>
    /// Enum representing the types of Transaction Accounts
    /// </summary>
    private enum TransactionAccountType
    {
        Debit,
        Credit
    }
}