using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.Exceptions;

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
    public AccountingPeriodId AccountingPeriod { get; private set; }

    /// <summary>
    /// Date for this Transaction
    /// </summary>
    public DateOnly Date { get; internal set; }

    /// <summary>
    /// Location for this Transaction
    /// </summary>
    public string Location { get; internal set; }

    /// <summary>
    /// Description for this Transaction
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Debit Account for this Transaction
    /// </summary>
    public TransactionAccount? DebitAccount { get; internal set; }

    /// <summary>
    /// Credit Account for this Transaction
    /// </summary>
    public TransactionAccount? CreditAccount { get; internal set; }

    /// <summary>
    /// Account ID of the Account that created this transaction when it was created, or null
    /// </summary>
    public AccountId? InitialAccountTransaction { get; internal set; }

    /// <summary>
    /// Attempts to apply this Transaction to the provided existing Account Balance as of the specified date
    /// </summary>
    public bool TryApplyToAccountBalance(
        AccountBalance existingAccountBalance,
        DateOnly date,
        [NotNullWhen(true)] out AccountBalance? newAccountBalance,
        out IEnumerable<Exception> exceptions)
    {
        newAccountBalance = null;
        exceptions = [];

        if (DebitAccount?.Account == existingAccountBalance.AccountId)
        {
            return TryApplyToTransactionAccountBalance(DebitAccount, TransactionAccountType.Debit, existingAccountBalance, date, out newAccountBalance, out exceptions);
        }
        if (CreditAccount?.Account == existingAccountBalance.AccountId)
        {
            return TryApplyToTransactionAccountBalance(CreditAccount, TransactionAccountType.Credit, existingAccountBalance, date, out newAccountBalance, out exceptions);
        }
        exceptions = exceptions.Append(new InvalidAccountException("Transaction does not involve the account for the provided balance."));
        return false;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Transaction(CreateTransactionRequest request)
        : base(new TransactionId(Guid.NewGuid()))
    {
        AccountingPeriod = request.AccountingPeriod;
        Date = request.Date;
        Location = request.Location;
        Description = request.Description;
        DebitAccount = request.DebitAccount != null ? new TransactionAccount(request.DebitAccount.Account.Id, request.DebitAccount.FundAmounts) : null;
        CreditAccount = request.CreditAccount != null ? new TransactionAccount(request.CreditAccount.Account.Id, request.CreditAccount.FundAmounts) : null;
        InitialAccountTransaction = request.IsInitialTransactionForCreditAccount ? CreditAccount?.Account : null;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Transaction() : base()
    {
        AccountingPeriod = null!;
        Location = null!;
        Description = null!;
        DebitAccount = null;
        CreditAccount = null;
    }

    /// <summary>
    /// Attempts to apply this Transaction to the provided existing Account Balance as of the specified date
    /// </summary>
    private bool TryApplyToTransactionAccountBalance(
        TransactionAccount transactionAccount,
        TransactionAccountType transactionAccountType,
        AccountBalance existingAccountBalance,
        DateOnly date,
        [NotNullWhen(true)] out AccountBalance? newAccountBalance,
        out IEnumerable<Exception> exceptions)
    {
        newAccountBalance = existingAccountBalance;
        exceptions = [];

        if (Date == date)
        {
            IEnumerable<FundAmount> pendingFundAmounts = transactionAccount.FundAmounts.Select(fundAmount => new FundAmount
            {
                FundId = fundAmount.FundId,
                Amount = transactionAccountType == TransactionAccountType.Debit ? -fundAmount.Amount : fundAmount.Amount
            });
            if (!newAccountBalance.TryAddNewPendingAmounts(pendingFundAmounts, out newAccountBalance, out IEnumerable<Exception> addExceptions))
            {
                exceptions = exceptions.Concat(addExceptions);
                return false;
            }
        }
        if (transactionAccount.PostedDate == date)
        {
            IEnumerable<FundAmount> pendingFundAmounts = transactionAccount.FundAmounts.Select(fundAmount => new FundAmount
            {
                FundId = fundAmount.FundId,
                Amount = transactionAccountType == TransactionAccountType.Debit ? fundAmount.Amount : -fundAmount.Amount
            });
            if (!newAccountBalance.TryAddNewPendingAmounts(pendingFundAmounts, out newAccountBalance, out IEnumerable<Exception> addExceptions))
            {
                exceptions = exceptions.Concat(addExceptions);
                return false;
            }
            IEnumerable<FundAmount> fundAmounts = transactionAccount.FundAmounts.Select(fundAmount => new FundAmount
            {
                FundId = fundAmount.FundId,
                Amount = transactionAccountType == TransactionAccountType.Debit ? -fundAmount.Amount : fundAmount.Amount
            });
            if (!newAccountBalance.TryAddNewAmounts(fundAmounts, out newAccountBalance, out IEnumerable<Exception> addExceptions2))
            {
                exceptions = exceptions.Concat(addExceptions2);
                return false;
            }
            return true;
        }
        if (newAccountBalance == null)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("Transaction is not applicable for the provided date."));
            return false;
        }
        return true;
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