using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.Exceptions;

namespace Domain.Transactions;

/// <summary>
/// Service for managing Transactions
/// </summary>
public class TransactionService(
    AccountBalanceService accountBalanceService,
    FundBalanceService fundBalanceService,
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
        transaction = new Transaction(request, transactionRepository.GetNextSequenceForDate(request.Date));
        accountBalanceService.AddTransaction(transaction);
        fundBalanceService.AddTransaction(transaction);
        transactionRepository.Add(transaction);
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
        if (!ValidateDate(accountingPeriodRepository.GetById(transaction.AccountingPeriodId),
                transaction.DebitAccount != null ? accountRepository.GetById(transaction.DebitAccount.AccountId) : null,
                transaction.CreditAccount != null ? accountRepository.GetById(transaction.CreditAccount.AccountId) : null,
                request.Date,
                out IEnumerable<Exception> dateExceptions))
        {
            exceptions = exceptions.Concat(dateExceptions);
        }
        if (!ValidateAccountFundAmounts(transaction.DebitAccount?.AccountId,
                request.DebitAccount?.FundAmounts.ToList(),
                transaction.CreditAccount?.AccountId,
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
            transaction.DebitAccount = new TransactionAccount(transaction, transaction.DebitAccount.AccountId, request.DebitAccount.FundAmounts);
        }
        if (transaction.CreditAccount != null && request.CreditAccount != null)
        {
            transaction.CreditAccount = new TransactionAccount(transaction, transaction.CreditAccount.AccountId, request.CreditAccount.FundAmounts);
        }
        accountBalanceService.UpdateTransaction(transaction);
        fundBalanceService.UpdateTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Transaction within an Account
    /// </summary>
    public bool TryPost(Transaction transaction, AccountId account, DateOnly postedDate, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidatePosting(transaction, account, postedDate, out TransactionAccount? transactionAccount, out IEnumerable<Exception> postingExceptions))
        {
            exceptions = exceptions.Concat(postingExceptions);
            return false;
        }
        transactionAccount.PostedDate = postedDate;
        accountBalanceService.PostTransaction(transaction, transactionAccount);
        fundBalanceService.PostTransaction(transaction, transactionAccount);
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Transaction
    /// </summary>
    public bool TryDelete(Transaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (accountBeingDeleted != transaction.GeneratedByAccountId && !ValidateNotInitialTransactionForAccount(transaction, out IEnumerable<Exception> initialTransactionExceptions))
        {
            exceptions = exceptions.Concat(initialTransactionExceptions);
        }
        if (!ValidateAccountingPeriodNotClosed(accountingPeriodRepository.GetById(transaction.AccountingPeriodId), out IEnumerable<Exception> accountingPeriodExceptions))
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
        accountBalanceService.DeleteTransaction(transaction);
        fundBalanceService.DeleteTransaction(transaction);
        transactionRepository.Delete(transaction);
        return true;
    }

    /// <summary>
    /// Validates that the Accounting Period is not closed
    /// </summary>
    private static bool ValidateAccountingPeriodNotClosed(AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

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

        if (!ValidateAccountingPeriodNotClosed(request.AccountingPeriod, out IEnumerable<Exception> accountingPeriodExceptions))
        {
            exceptions = exceptions.Concat(accountingPeriodExceptions);
        }
        if (request.DebitAccount != null)
        {
            AccountingPeriod debitAccountInitialPeriod = accountingPeriodRepository.GetById(request.DebitAccount.Account.InitialAccountingPeriodId);
            if (request.AccountingPeriod.PeriodStartDate < debitAccountInitialPeriod.PeriodStartDate)
            {
                exceptions = exceptions.Append(new InvalidAccountingPeriodException("The Debit Account did not exist during the provided Accounting Period."));
            }
        }
        if (request.CreditAccount != null)
        {
            AccountingPeriod creditAccountInitialPeriod = accountingPeriodRepository.GetById(request.CreditAccount.Account.InitialAccountingPeriodId);
            if (request.AccountingPeriod.PeriodStartDate < creditAccountInitialPeriod.PeriodStartDate)
            {
                exceptions = exceptions.Append(new InvalidAccountingPeriodException("The Credit Account did not exist during the provided Accounting Period."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Date for this Transaction
    /// </summary>
    private static bool ValidateDate(
        AccountingPeriod accountingPeriod,
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
        if (!accountingPeriod.IsDateInPeriod(date))
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction Date must be in a month adjacent to the Accounting Period month."));
        }
        if (debitAccount != null && date < debitAccount.InitialDate)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction Date is before the Debit Account was created."));
        }
        if (creditAccount != null && date < creditAccount.InitialDate)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction Date is before the Credit Account was created."));
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
            exceptions = exceptions.Append(new InvalidDebitAccountException("The Debit Account and its Fund Amounts must both be provided or both be null."));
        }
        if (creditAccount == null != (creditFundAmounts == null))
        {
            exceptions = exceptions.Append(new InvalidCreditAccountException("The Credit Account and its Fund Amounts must both be provided or both be null."));
        }
        if (debitFundAmounts == null && creditFundAmounts == null)
        {
            exceptions = exceptions.Append(new InvalidDebitAccountException("At least one of Debit Account or Credit Account must be provided."));
            exceptions = exceptions.Append(new InvalidCreditAccountException("At least one of Debit Account or Credit Account must be provided."));
        }
        if (debitAccount == creditAccount && debitFundAmounts != null && creditFundAmounts != null &&
            debitFundAmounts.Any(debitFund => creditFundAmounts.Select(fundAmount => fundAmount.FundId).Contains(debitFund.FundId)))
        {
            exceptions = exceptions.Append(new InvalidDebitAccountException("For a Transaction with a single Account, the same Fund cannot be both debited and credited"));
            exceptions = exceptions.Append(new InvalidCreditAccountException("For a Transaction with a single Account, the same Fund cannot be both debited and credited"));
        }
        if (debitFundAmounts != null)
        {
            if (debitFundAmounts.Count == 0)
            {
                exceptions = exceptions.Append(new InvalidDebitAccountException("The Debit Account must have at least one Fund Amount."));
            }
            if (debitFundAmounts.Any(fundAmount => fundAmount.Amount <= 0))
            {
                exceptions = exceptions.Append(new InvalidDebitAccountException("All Fund Amounts for the Debit Account must be greater than zero."));
            }
        }
        if (creditFundAmounts != null)
        {
            if (creditFundAmounts.Count == 0)
            {
                exceptions = exceptions.Append(new InvalidCreditAccountException("The Credit Account must have at least one Fund Amount."));
            }
            if (creditFundAmounts.Any(fundAmount => fundAmount.Amount <= 0))
            {
                exceptions = exceptions.Append(new InvalidCreditAccountException("All Fund Amounts for the Credit Account must be greater than zero."));
            }
        }
        if (debitFundAmounts != null && creditFundAmounts != null)
        {
            decimal totalDebit = debitFundAmounts.Sum(fundAmount => fundAmount.Amount);
            decimal totalCredit = creditFundAmounts.Sum(fundAmount => fundAmount.Amount);
            if (totalDebit != totalCredit)
            {
                exceptions = exceptions.Append(new InvalidDebitAccountException("The total Debit Fund Amounts must equal the total Credit Fund Amounts."));
                exceptions = exceptions.Append(new InvalidCreditAccountException("The total Debit Fund Amounts must equal the total Credit Fund Amounts."));
            }
        }
        if (existingTransaction != null)
        {
            if (existingTransaction.DebitAccount == null != (debitFundAmounts == null))
            {
                exceptions = exceptions.Append(new InvalidDebitAccountException("The Debit Account cannot be added or removed when updating a Transaction."));
            }
            if (existingTransaction.CreditAccount == null != (creditFundAmounts == null))
            {
                exceptions = exceptions.Append(new InvalidCreditAccountException("The Credit Account cannot be added or removed when updating a Transaction."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the posting of this Transaction within an Account
    /// </summary>
    private bool ValidatePosting(
        Transaction transaction,
        AccountId account,
        DateOnly postedDate,
        [NotNullWhen(true)] out TransactionAccount? transactionAccount,
        out IEnumerable<Exception> exceptions)
    {
        transactionAccount = null;
        exceptions = [];

        if (transaction.DebitAccount != null && transaction.DebitAccount.AccountId == account)
        {
            transactionAccount = transaction.DebitAccount;
        }
        else if (transaction.CreditAccount != null && transaction.CreditAccount.AccountId == account)
        {
            transactionAccount = transaction.CreditAccount;
        }
        else
        {
            exceptions = exceptions.Append(new InvalidDebitAccountException("The Transaction does not involve the specified Account."));
            exceptions = exceptions.Append(new InvalidCreditAccountException("The Transaction does not involve the specified Account."));
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
        else
        {
            AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
            int monthDifference = Math.Abs(((accountingPeriod.Year - postedDate.Year) * 12) + accountingPeriod.Month - postedDate.Month);
            if (monthDifference > 1)
            {
                exceptions = exceptions.Append(new InvalidTransactionDateException("The posted date must be in a month adjacent to the Accounting Period month."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates that the Transaction is not the initial transaction for an Account
    /// </summary>
    private static bool ValidateNotInitialTransactionForAccount(Transaction transaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (transaction.GeneratedByAccountId != null)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("The Transaction is the initial transaction for an Account."));
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

        if (transaction.DebitAccount != null && accountBeingDeleted != transaction.DebitAccount.AccountId && transaction.DebitAccount.PostedDate != null)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction has been posted to the Debit Account."));
        }
        if (transaction.CreditAccount != null && accountBeingDeleted != transaction.CreditAccount.AccountId && transaction.CreditAccount.PostedDate != null)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("The Transaction has been posted to the Credit Account."));
        }
        return !exceptions.Any();
    }
}