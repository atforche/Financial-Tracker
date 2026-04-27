using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Service for managing Account Transactions
/// </summary>
public class AccountTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService,
        accountingPeriodBalanceService,
        fundBalanceService,
        accountingPeriodRepository,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Account Transaction
    /// </summary>
    public bool TryCreate(
        CreateAccountTransactionRequest request,
        [NotNullWhen(true)] out AccountTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new AccountTransaction(request, sequence);
        AddTransaction(transaction);
        if (transaction.DebitAccountId != null && request.DebitPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        if (transaction.CreditAccountId != null && request.CreditPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.CreditAccountId, request.CreditPostedDate.Value, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        if (exceptions.Any())
        {
            transaction = null;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Account Transaction
    /// </summary>
    public bool TryUpdate(
        AccountTransaction transaction,
        UpdateAccountTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUpdate(transaction, request, out exceptions))
        {
            return false;
        }
        UpdateTransaction(transaction, request);
        if (transaction.DebitAccountId != null && request.DebitPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        if (transaction.CreditAccountId != null && request.CreditPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.CreditAccountId, request.CreditPostedDate.Value, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Attempts to post an existing Account Transaction to a specific Account
    /// </summary>
    public bool TryPost(
        AccountTransaction transaction,
        AccountId accountId,
        DateOnly postedDate,
        out IEnumerable<Exception> exceptions)
    {
        if (!ValidatePosting(transaction, accountId, postedDate, out exceptions))
        {
            return false;
        }
        if (accountId == transaction.DebitAccountId)
        {
            transaction.DebitPostedDate = postedDate;
        }
        else if (accountId == transaction.CreditAccountId)
        {
            transaction.CreditPostedDate = postedDate;
        }
        PostTransaction(transaction, accountId);
        return true;
    }

    /// <summary>
    /// Attempts to unpost an existing Account Transaction
    /// </summary>
    public bool TryUnpost(AccountTransaction transaction, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUnposting(transaction, out exceptions))
        {
            return false;
        }
        transaction.DebitPostedDate = null;
        transaction.CreditPostedDate = null;
        UnpostTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Account Transaction
    /// </summary>
    public bool TryDelete(AccountTransaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateDelete(transaction, accountBeingDeleted, out exceptions))
        {
            return false;
        }
        DeleteTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Account Transaction
    /// </summary>
    private bool ValidateCreate(CreateAccountTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(
                request,
                new List<Account?> { request.DebitAccount, request.CreditAccount }.OfType<Account>().ToList(),
                [],
                out exceptions);

        if (!ValidateAccounts(request, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        AccountingPeriod accountingPeriod = AccountingPeriodRepository.GetById(request.AccountingPeriodId);
        if (!ValidatePostedDates(
                accountingPeriod,
                request.DebitAccount,
                request.DebitPostedDate,
                request.CreditAccount,
                request.CreditPostedDate,
                out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to update an existing Account Transaction
    /// </summary>
    private bool ValidateUpdate(
        AccountTransaction transaction,
        UpdateAccountTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        AccountingPeriod accountingPeriod = AccountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        Account? debitAccount = transaction.DebitAccountId != null ? accountRepository.GetById(transaction.DebitAccountId) : null;
        Account? creditAccount = transaction.CreditAccountId != null ? accountRepository.GetById(transaction.CreditAccountId) : null;
        _ = ValidateUpdate(transaction, request, new List<Account?> { debitAccount, creditAccount }.OfType<Account>().ToList(), out exceptions);

        if (transaction.GeneratedByAccountId != null)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transactions generated by an Account cannot be updated"));
        }
        if (!ValidatePostedDates(
                accountingPeriod,
                debitAccount,
                request.DebitPostedDate,
                creditAccount,
                request.CreditPostedDate,
                out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (transaction.DebitPostedDate.HasValue || transaction.CreditPostedDate.HasValue)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction has already been posted and cannot be updated"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to unpost an existing Account Transaction
    /// </summary>
    protected bool ValidateUnposting(AccountTransaction transaction, out IEnumerable<Exception> exceptions)
    {
        _ = base.ValidateUnposting(transaction, out exceptions);

        if (transaction.GeneratedByAccountId != null)
        {
            exceptions = exceptions.Append(new UnableToUnpostException("Transaction was auto-generated by an account and cannot be unposted"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to delete an existing Account Transaction
    /// </summary>
    protected bool ValidateDelete(AccountTransaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateDelete(transaction, out exceptions);

        if (transaction.GeneratedByAccountId != null && transaction.GeneratedByAccountId != accountBeingDeleted)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Transaction was auto-generated by an account and cannot be deleted directly"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Accounts for this Account Transaction
    /// </summary>
    private static bool ValidateAccounts(CreateAccountTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.DebitAccount != null && request.CreditAccount != null)
        {
            if (request.DebitAccount.Id == request.CreditAccount.Id)
            {
                exceptions = exceptions.Append(new InvalidAccountException("Debit and Credit Accounts must be different"));
            }
            if (request.DebitAccount.Type.IsTracked() && !request.CreditAccount.Type.IsTracked())
            {
                exceptions = exceptions.Append(new InvalidAccountException("An Account Transaction cannot transfer between a tracked account and an untracked account"));
            }
            if (!request.DebitAccount.Type.IsTracked() && request.CreditAccount.Type.IsTracked())
            {
                exceptions = exceptions.Append(new InvalidAccountException("An Account Transaction cannot transfer between a tracked account and an untracked account"));
            }
        }
        else if (request.DebitAccount != null || request.CreditAccount != null)
        {
            if (request.DebitAccount != null && !request.DebitAccount.Type.IsTracked())
            {
                exceptions = exceptions.Append(new InvalidAccountException("A one-sided Account Transaction cannot debit money from an untracked account"));
            }
            if (request.CreditAccount != null && !request.CreditAccount.Type.IsTracked())
            {
                exceptions = exceptions.Append(new InvalidAccountException("A one-sided Account Transaction cannot credit money to an untracked account"));
            }
        }
        else
        {
            exceptions = exceptions.Append(new InvalidAccountException("At least one account must be provided"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Posted Dates for this Account Transaction
    /// </summary>
    private static bool ValidatePostedDates(
        AccountingPeriod accountingPeriod,
        Account? debitAccount,
        DateOnly? debitPostedDate,
        Account? creditAccount,
        DateOnly? creditPostedDate,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (debitPostedDate.HasValue && debitAccount == null)
        {
            exceptions = exceptions.Append(new InvalidDateException("A posted date cannot be provided for the debit account if no debit account is provided"));
        }
        if (creditPostedDate.HasValue && creditAccount == null)
        {
            exceptions = exceptions.Append(new InvalidDateException("A posted date cannot be provided for the credit account if no credit account is provided"));
        }
        if (debitAccount != null && !ValidatePostedDate(accountingPeriod, debitAccount, debitPostedDate, out IEnumerable<Exception> debitPostedDateExceptions))
        {
            exceptions = exceptions.Concat(debitPostedDateExceptions);
        }
        if (creditAccount != null && !ValidatePostedDate(accountingPeriod, creditAccount, creditPostedDate, out IEnumerable<Exception> creditPostedDateExceptions))
        {
            exceptions = exceptions.Concat(creditPostedDateExceptions);
        }
        return !exceptions.Any();
    }
}