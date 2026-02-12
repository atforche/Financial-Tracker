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

        if (DebitAccount?.AccountId == existingAccountBalance.AccountId)
        {
            return TryApplyToAccountBalancePrivate(DebitAccount, TransactionAccountType.Debit, existingAccountBalance, date, out newAccountBalance, out exceptions);
        }
        if (CreditAccount?.AccountId == existingAccountBalance.AccountId)
        {
            return TryApplyToAccountBalancePrivate(CreditAccount, TransactionAccountType.Credit, existingAccountBalance, date, out newAccountBalance, out exceptions);
        }
        exceptions = exceptions.Append(new InvalidAccountException("Transaction does not involve the account for the provided balance."));
        return false;
    }

    /// <summary>
    /// Attempts to apply this Transaction to the provided existing Fund Balance as of the specified date
    /// </summary>
    public bool TryApplyToFundBalance(
        FundBalance existingFundBalance,
        DateOnly date,
        [NotNullWhen(true)] out FundBalance? newFundBalance,
        out IEnumerable<Exception> exceptions)
    {
        newFundBalance = null;
        exceptions = [];

        FundAmount? debitFundAmount = DebitAccount?.FundAmounts.SingleOrDefault(fundAmount => fundAmount.FundId == existingFundBalance.FundId);
        if (DebitAccount != null && debitFundAmount != null)
        {
            if (!TryApplyToFundBalancePrivate(DebitAccount, debitFundAmount, TransactionAccountType.Debit, existingFundBalance, date, out newFundBalance, out exceptions))
            {
                return false;
            }
        }
        FundAmount? creditFundAmount = CreditAccount?.FundAmounts.SingleOrDefault(fundAmount => fundAmount.FundId == existingFundBalance.FundId);
        if (CreditAccount != null && creditFundAmount != null)
        {
            if (!TryApplyToFundBalancePrivate(CreditAccount, creditFundAmount, TransactionAccountType.Credit, newFundBalance ?? existingFundBalance, date, out newFundBalance, out exceptions))
            {
                return false;
            }
        }
        if (newFundBalance == null)
        {
            exceptions = exceptions.Append(new InvalidFundException("Transaction does not involve the fund for the provided balance."));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Transaction(CreateTransactionRequest request, int sequence)
        : base(new TransactionId(Guid.NewGuid()))
    {
        AccountingPeriod = request.AccountingPeriod;
        Date = request.Date;
        Sequence = sequence;
        Location = request.Location;
        Description = request.Description;
        DebitAccount = request.DebitAccount != null ? new TransactionAccount(this, request.DebitAccount.Account.Id, request.DebitAccount.FundAmounts) : null;
        CreditAccount = request.CreditAccount != null ? new TransactionAccount(this, request.CreditAccount.Account.Id, request.CreditAccount.FundAmounts) : null;
        InitialAccountTransaction = request.IsInitialTransactionForCreditAccount ? CreditAccount?.AccountId : null;
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
    private bool TryApplyToAccountBalancePrivate(
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
                Amount = fundAmount.Amount
            });
            if (transactionAccountType == TransactionAccountType.Debit && !newAccountBalance.TryAddNewPendingDebits(pendingFundAmounts, out newAccountBalance, out IEnumerable<Exception> addExceptions))
            {
                exceptions = exceptions.Concat(addExceptions);
                return false;
            }
            if (transactionAccountType == TransactionAccountType.Credit && !newAccountBalance.TryAddNewPendingCredits(pendingFundAmounts, out newAccountBalance, out addExceptions))
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
                Amount = -fundAmount.Amount
            });
            if (transactionAccountType == TransactionAccountType.Debit && !newAccountBalance.TryAddNewPendingDebits(pendingFundAmounts, out newAccountBalance, out IEnumerable<Exception> addExceptions))
            {
                exceptions = exceptions.Concat(addExceptions);
                return false;
            }
            if (transactionAccountType == TransactionAccountType.Credit && !newAccountBalance.TryAddNewPendingCredits(pendingFundAmounts, out newAccountBalance, out addExceptions))
            {
                exceptions = exceptions.Concat(addExceptions);
                return false;
            }
            IEnumerable<FundAmount> fundAmounts = transactionAccount.FundAmounts.Select(fundAmount => new FundAmount
            {
                FundId = fundAmount.FundId,
                Amount = transactionAccountType == TransactionAccountType.Debit ? -1 * fundAmount.Amount : fundAmount.Amount
            });
            if (!newAccountBalance.TryAddNewAmounts(fundAmounts, out newAccountBalance, out IEnumerable<Exception> addExceptions2))
            {
                exceptions = exceptions.Concat(addExceptions2);
                return false;
            }
        }
        if (newAccountBalance == null)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("Transaction is not applicable for the provided date."));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to apply this Transaction to the provided existing Fund Balance as of the specified date
    /// </summary>
    private bool TryApplyToFundBalancePrivate(
        TransactionAccount transactionAccount,
        FundAmount fundAmount,
        TransactionAccountType transactionAccountType,
        FundBalance existingFundBalance,
        DateOnly date,
        [NotNullWhen(true)] out FundBalance? newFundBalance,
        out IEnumerable<Exception> exceptions)
    {
        newFundBalance = existingFundBalance;
        exceptions = [];

        if (Date == date)
        {
            var pendingAccountAmount = new AccountAmount
            {
                AccountId = transactionAccount.AccountId,
                Amount = fundAmount.Amount
            };
            newFundBalance = transactionAccountType == TransactionAccountType.Debit
                ? newFundBalance.AddNewPendingDebits(pendingAccountAmount)
                : newFundBalance.AddNewPendingCredits(pendingAccountAmount);
        }
        if (transactionAccount.PostedDate == date)
        {
            var pendingAccountAmount = new AccountAmount
            {
                AccountId = transactionAccount.AccountId,
                Amount = -fundAmount.Amount
            };
            newFundBalance = transactionAccountType == TransactionAccountType.Debit
                ? newFundBalance.AddNewPendingDebits(pendingAccountAmount)
                : newFundBalance.AddNewPendingCredits(pendingAccountAmount);
            var accountAmount = new AccountAmount
            {
                AccountId = transactionAccount.AccountId,
                Amount = transactionAccountType == TransactionAccountType.Debit ? -1 * fundAmount.Amount : fundAmount.Amount
            };
            newFundBalance = newFundBalance.AddNewAmount(accountAmount);
        }
        if (newFundBalance == null)
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