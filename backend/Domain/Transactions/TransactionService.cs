using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Accounts.Exceptions;
using Domain.Funds;
using Domain.Transactions.Exceptions;

namespace Domain.Transactions;

/// <summary>
/// Service for managing Transactions
/// </summary>
public class TransactionService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Transaction
    /// </summary>
    public bool TryCreate(CreateTransactionRequest request, [NotNullWhen(true)] out Transaction? transaction, out IEnumerable<Exception> exceptions)
    {
        transaction = null;
        exceptions = [];

        if (!ValidateAccountingPeriod(request, out IEnumerable<Exception> accountingPeriodExceptions))
        {
            exceptions = exceptions.Concat(accountingPeriodExceptions);
        }
        if (!ValidateDate(request.AccountingPeriod,
                request.DebitAccount?.Account,
                request.CreditAccount?.Account,
                request.Date,
                out IEnumerable<Exception> dateExceptions))
        {
            exceptions = exceptions.Concat(dateExceptions);
        }
        if (!ValidateAccountFundAmounts(request.DebitAccount?.Account.Id,
                request.DebitAccount?.FundAmounts.ToList(),
                request.CreditAccount?.Account.Id,
                request.CreditAccount?.FundAmounts.ToList(),
                null,
                out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (exceptions.Any())
        {
            return false;
        }
        transaction = new Transaction(request);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Transaction
    /// </summary>
    public bool TryUpdate(Transaction transaction, UpdateTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateNotInitialTransactionForAccount(transaction, out IEnumerable<Exception> initialTransactionExceptions))
        {
            exceptions = exceptions.Concat(initialTransactionExceptions);
        }
        if (!ValidateNotPosted(transaction, null, out IEnumerable<Exception> postedExceptions))
        {
            exceptions = exceptions.Concat(postedExceptions);
        }
        if (!ValidateDate(transaction.AccountingPeriod,
                transaction.DebitAccount != null ? accountRepository.FindById(transaction.DebitAccount.Account) : null,
                transaction.CreditAccount != null ? accountRepository.FindById(transaction.CreditAccount.Account) : null,
                request.Date,
                out IEnumerable<Exception> dateExceptions))
        {
            exceptions = exceptions.Concat(dateExceptions);
        }
        if (!ValidateAccountFundAmounts(transaction.DebitAccount?.Account,
                request.DebitAccount?.FundAmounts.ToList(),
                transaction.CreditAccount?.Account,
                request.CreditAccount?.FundAmounts.ToList(),
                transaction,
                out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (exceptions.Any())
        {
            return false;
        }
        transaction.Date = request.Date;
        transaction.Location = request.Location;
        transaction.Description = request.Description;
        if (transaction.DebitAccount != null && request.DebitAccount != null)
        {
            transaction.DebitAccount = new TransactionAccount(transaction.DebitAccount.Account, request.DebitAccount.FundAmounts);
        }
        if (transaction.CreditAccount != null && request.CreditAccount != null)
        {
            transaction.CreditAccount = new TransactionAccount(transaction.CreditAccount.Account, request.CreditAccount.FundAmounts);
        }
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Transaction within an Account
    /// </summary>
    public static bool TryPost(Transaction transaction, AccountId account, DateOnly postedDate, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidatePosting(transaction, account, postedDate, out TransactionAccount? transactionAccount, out IEnumerable<Exception> postingExceptions))
        {
            exceptions = exceptions.Concat(postingExceptions);
            return false;
        }
        transactionAccount.PostedDate = postedDate;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Transaction
    /// </summary>
    public bool TryDelete(Transaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (accountBeingDeleted != transaction.InitialAccountTransaction && !ValidateNotInitialTransactionForAccount(transaction, out IEnumerable<Exception> initialTransactionExceptions))
        {
            exceptions = exceptions.Concat(initialTransactionExceptions);
        }
        if (!ValidateAccountingPeriodNotClosed(transaction.AccountingPeriod, out IEnumerable<Exception> accountingPeriodExceptions))
        {
            exceptions = exceptions.Concat(accountingPeriodExceptions);
        }
        if (!ValidateNotPosted(transaction, accountBeingDeleted, out IEnumerable<Exception> notPostedExceptions))
        {
            exceptions = exceptions.Concat(notPostedExceptions);
        }
        if (exceptions.Any())
        {
            return false;
        }
        transactionRepository.Delete(transaction);
        return true;
    }

    /// <summary>
    /// Validates that the Accounting Period is not closed
    /// </summary>
    private bool ValidateAccountingPeriodNotClosed(AccountingPeriodId accountingPeriodId, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodId);
        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The Accounting Period is closed."));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Validates the Accounting Period for this Transaction
    /// </summary>
    private bool ValidateAccountingPeriod(CreateTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(request.AccountingPeriod);
        if (!ValidateAccountingPeriodNotClosed(request.AccountingPeriod, out IEnumerable<Exception> accountingPeriodExceptions))
        {
            exceptions = exceptions.Concat(accountingPeriodExceptions);
        }
        if (request.DebitAccount?.Account.InitialTransaction != null)
        {
            Transaction debitAccountInitialTransaction = transactionRepository.FindById(request.DebitAccount.Account.InitialTransaction);
            AccountingPeriod debitAccountInitialPeriod = accountingPeriodRepository.FindById(debitAccountInitialTransaction.AccountingPeriod);
            if (accountingPeriod.PeriodStartDate < debitAccountInitialPeriod.PeriodStartDate)
            {
                exceptions = exceptions.Append(new InvalidAccountingPeriodException("The Debit Account did not exist during the provided Accounting Period."));
            }
        }
        if (request.CreditAccount?.Account.InitialTransaction != null)
        {
            Transaction creditAccountInitialTransaction = transactionRepository.FindById(request.CreditAccount.Account.InitialTransaction);
            AccountingPeriod creditAccountInitialPeriod = accountingPeriodRepository.FindById(creditAccountInitialTransaction.AccountingPeriod);
            if (accountingPeriod.PeriodStartDate < creditAccountInitialPeriod.PeriodStartDate)
            {
                exceptions = exceptions.Append(new InvalidAccountingPeriodException("The Credit Account did not exist during the provided Accounting Period."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Date for this Transaction
    /// </summary>
    private bool ValidateDate(
        AccountingPeriodId accountingPeriodId,
        Account? debitAccount,
        Account? creditAccount,
        DateOnly date,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (date == DateOnly.MinValue)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction must have a valid Date."));
        }
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodId);
        int monthDifference = Math.Abs(((accountingPeriod.Year - date.Year) * 12) + accountingPeriod.Month - date.Month);
        if (monthDifference > 1)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction Date must be in a month adjacent to the Accounting Period month."));
        }
        if (debitAccount?.InitialTransaction != null)
        {
            Transaction debitAccountInitialTransaction = transactionRepository.FindById(debitAccount.InitialTransaction);
            if (date < debitAccountInitialTransaction.Date)
            {
                exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction Date is before the Debit Account was created."));
            }
        }
        if (creditAccount?.InitialTransaction != null)
        {
            Transaction creditAccountInitialTransaction = transactionRepository.FindById(creditAccount.InitialTransaction);
            if (date < creditAccountInitialTransaction.Date)
            {
                exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction Date is before the Credit Account was created."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the fund amounts for each Account for this Transaction
    /// </summary>
    private static bool ValidateAccountFundAmounts(
        AccountId? debitAccount,
        List<FundAmount>? debitFundAmounts,
        AccountId? creditAccount,
        List<FundAmount>? creditFundAmounts,
        Transaction? existingTransaction,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (debitAccount == null != (debitFundAmounts == null))
        {
            exceptions = exceptions.Append(new InvalidAccountException("The Debit Account and its Fund Amounts must both be provided or both be null."));
        }
        if (creditAccount == null != (creditFundAmounts == null))
        {
            exceptions = exceptions.Append(new InvalidAccountException("The Credit Account and its Fund Amounts must both be provided or both be null."));
        }
        if (debitFundAmounts == null && creditFundAmounts == null)
        {
            exceptions = exceptions.Append(new InvalidAccountException("At least one of Debit Account or Credit Account must be provided."));
        }
        if (debitAccount == creditAccount && debitFundAmounts != null && creditFundAmounts != null &&
            debitFundAmounts.Any(debitFund => creditFundAmounts.Select(fundAmount => fundAmount.FundId).Contains(debitFund.FundId)))
        {
            exceptions = exceptions.Append(new InvalidAccountException("For a Transaction with a single Account, the same Fund cannot be both debited and credited"));
        }
        if (debitFundAmounts != null)
        {
            if (debitFundAmounts.Count == 0)
            {
                exceptions = exceptions.Append(new InvalidFundAmountException("The Debit Account must have at least one Fund Amount."));
            }
            if (debitFundAmounts.Any(fundAmount => fundAmount.Amount <= 0))
            {
                exceptions = exceptions.Append(new InvalidFundAmountException("All Fund Amounts for the Debit Account must be greater than zero."));
            }
        }
        if (creditFundAmounts != null)
        {
            if (creditFundAmounts.Count == 0)
            {
                exceptions = exceptions.Append(new InvalidFundAmountException("The Credit Account must have at least one Fund Amount."));
            }
            if (creditFundAmounts.Any(fundAmount => fundAmount.Amount <= 0))
            {
                exceptions = exceptions.Append(new InvalidFundAmountException("All Fund Amounts for the Credit Account must be greater than zero."));
            }
        }
        if (debitFundAmounts != null && creditFundAmounts != null)
        {
            decimal totalDebit = debitFundAmounts.Sum(fundAmount => fundAmount.Amount);
            decimal totalCredit = creditFundAmounts.Sum(fundAmount => fundAmount.Amount);
            if (totalDebit != totalCredit)
            {
                exceptions = exceptions.Append(new InvalidFundAmountException("The total Debit Fund Amounts must equal the total Credit Fund Amounts."));
            }
        }
        if (existingTransaction != null)
        {
            if (existingTransaction.DebitAccount == null != (debitFundAmounts == null))
            {
                exceptions = exceptions.Append(new InvalidAccountException("The Debit Account cannot be added or removed when updating a Transaction."));
            }
            if (existingTransaction.CreditAccount == null != (creditFundAmounts == null))
            {
                exceptions = exceptions.Append(new InvalidAccountException("The Credit Account cannot be added or removed when updating a Transaction."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the posting of this Transaction within an Account
    /// </summary>
    private static bool ValidatePosting(
        Transaction transaction,
        AccountId account,
        DateOnly postedDate,
        [NotNullWhen(true)] out TransactionAccount? transactionAccount,
        out IEnumerable<Exception> exceptions)
    {
        transactionAccount = null;
        exceptions = [];

        if (transaction.DebitAccount != null && transaction.DebitAccount.Account == account)
        {
            transactionAccount = transaction.DebitAccount;
        }
        else if (transaction.CreditAccount != null && transaction.CreditAccount.Account == account)
        {
            transactionAccount = transaction.CreditAccount;
        }
        else
        {
            exceptions = exceptions.Append(new InvalidAccountException("The Transaction does not involve the specified Account."));
            return false;
        }
        if (transactionAccount.PostedDate != null)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction has already been posted to this Account."));
        }
        else if (postedDate < transaction.Date)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The posted date is before the Transaction date."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates that the Transaction is not the initial transaction for an Account
    /// </summary>
    private static bool ValidateNotInitialTransactionForAccount(Transaction transaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (transaction.InitialAccountTransaction != null)
        {
            exceptions = exceptions.Append(new InvalidAccountException("The Transaction is the initial transaction for an Account."));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Validates that the Transaction has not been posted to either Account
    /// </summary>
    private static bool ValidateNotPosted(Transaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (transaction.DebitAccount != null && accountBeingDeleted != transaction.DebitAccount.Account && transaction.DebitAccount.PostedDate != null)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction has been posted to the Debit Account."));
        }
        if (transaction.CreditAccount != null && accountBeingDeleted != transaction.CreditAccount.Account && transaction.CreditAccount.PostedDate != null)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction has been posted to the Credit Account."));
        }
        return !exceptions.Any();
    }
}