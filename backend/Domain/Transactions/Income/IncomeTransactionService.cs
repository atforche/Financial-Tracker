using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Transactions.Income;

/// <summary>
/// Service for managing Income Transactions
/// </summary>
public class IncomeTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IFundRepository fundRepository,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService,
        accountingPeriodBalanceService,
        fundBalanceService,
        accountingPeriodRepository,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Income Transaction
    /// </summary>
    public bool TryCreate(
        CreateIncomeTransactionRequest request,
        [NotNullWhen(true)] out IncomeTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(
                request,
                new List<Account?> { request.CreditAccount, request.DebitAccount }.OfType<Account>().ToList(),
                out exceptions))
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new IncomeTransaction(request, sequence);
        AddTransaction(transaction);
        if (request.CreditPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.CreditAccountId, request.CreditPostedDate.Value, out exceptions))
            {
                transaction = null;
                return false;
            }
        }
        if (request.DebitPostedDate.HasValue && transaction.DebitAccountId != null)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out exceptions))
            {
                transaction = null;
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Income Transaction
    /// </summary>
    public bool TryUpdate(
        IncomeTransaction transaction,
        UpdateIncomeTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        Account creditAccount = accountRepository.GetById(transaction.CreditAccountId);
        Account? debitAccount = transaction.DebitAccountId != null ? accountRepository.GetById(transaction.DebitAccountId) : null;
        if (!ValidateUpdate(
                transaction,
                request,
                new List<Account?> { creditAccount, debitAccount }.OfType<Account>().ToList(),
                out exceptions))
        {
            return false;
        }
        transaction.UpdateFundAssignments(request.FundAssignments);
        UpdateTransaction(transaction, request);
        if (request.CreditPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.CreditAccountId, request.CreditPostedDate.Value, out exceptions))
            {
                return false;
            }
        }
        if (request.DebitPostedDate.HasValue && transaction.DebitAccountId != null)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out exceptions))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Income Transaction to a specific Account
    /// </summary>
    public bool TryPost(
        IncomeTransaction transaction,
        AccountId accountId,
        DateOnly postedDate,
        out IEnumerable<Exception> exceptions)
    {
        if (!ValidatePosting(transaction, accountId, postedDate, out exceptions))
        {
            return false;
        }
        if (accountId == transaction.CreditAccountId)
        {
            transaction.CreditPostedDate = postedDate;
        }
        else if (accountId == transaction.DebitAccountId)
        {
            transaction.DebitPostedDate = postedDate;
        }
        PostTransaction(transaction, accountId);
        return true;
    }

    /// <summary>
    /// Attempts to unpost an existing Income Transaction
    /// </summary>
    public bool TryUnpost(IncomeTransaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUnposting(transaction, accountBeingDeleted, out exceptions))
        {
            return false;
        }
        transaction.CreditPostedDate = null;
        transaction.DebitPostedDate = null;
        UnpostTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Income Transaction
    /// </summary>
    public bool TryDelete(IncomeTransaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateDelete(transaction, accountBeingDeleted, out exceptions))
        {
            return false;
        }
        DeleteTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Income Transaction
    /// </summary>
    protected bool ValidateCreate(
        CreateIncomeTransactionRequest request,
        IReadOnlyCollection<Account> accounts,
        out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(
            request,
            accounts,
            request.FundAssignments.Select(fundAmount => fundRepository.GetById(fundAmount.FundId)).ToList(),
            out exceptions);

        if (!ValidateAccount(request, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        AccountingPeriod accountingPeriod = AccountingPeriodRepository.GetById(request.AccountingPeriodId);
        if (!ValidatePostedDates(
                accountingPeriod,
                request.CreditAccount,
                request.CreditPostedDate,
                request.DebitAccount,
                request.DebitPostedDate,
                out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (!ValidateFundAssignments(request.Amount, request.FundAssignments, out IEnumerable<Exception> fundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(fundAssignmentExceptions);
        }
        if (!ValidateInitialTransactionForAccount(request.IsInitialTransactionForAccount, request.CreditAccount, out IEnumerable<Exception> initialTransactionForAccountExceptions))
        {
            exceptions = exceptions.Concat(initialTransactionForAccountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to update an existing Income Transaction
    /// </summary>
    protected bool ValidateUpdate(
        IncomeTransaction transaction,
        UpdateIncomeTransactionRequest request,
        IReadOnlyCollection<Account> accounts,
        out IEnumerable<Exception> exceptions)
    {
        _ = base.ValidateUpdate(transaction, request, accounts, out exceptions);

        AccountingPeriod accountingPeriod = AccountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        Account creditAccount = accountRepository.GetById(transaction.CreditAccountId);
        Account? debitAccount = transaction.DebitAccountId != null ? accountRepository.GetById(transaction.DebitAccountId) : null;
        if (transaction.GeneratedByAccountId != null)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction was auto-generated by an account and cannot be updated directly"));
        }
        if (!ValidatePostedDates(
                accountingPeriod,
                creditAccount,
                request.CreditPostedDate ?? transaction.CreditPostedDate,
                debitAccount,
                request.DebitPostedDate ?? transaction.DebitPostedDate,
                out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (transaction.CreditPostedDate.HasValue || transaction.DebitPostedDate.HasValue)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction has already been posted and cannot be updated"));
        }
        if (!ValidateFundAssignments(request.Amount, request.FundAssignments, out IEnumerable<Exception> fundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(fundAssignmentExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to unpost an existing Income Transaction
    /// </summary>
    protected bool ValidateUnposting(IncomeTransaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateUnposting(transaction, out exceptions);

        if (transaction.GeneratedByAccountId != null && accountBeingDeleted != transaction.GeneratedByAccountId)
        {
            exceptions = exceptions.Append(new UnableToUnpostException("Transaction was auto-generated by an account and cannot be unposted"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to delete an existing Income Transaction
    /// </summary>
    protected bool ValidateDelete(IncomeTransaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateDelete(transaction, out exceptions);

        if (transaction.GeneratedByAccountId != null && transaction.GeneratedByAccountId != accountBeingDeleted)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Transaction was auto-generated by an account and cannot be deleted directly"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Account for this Income Transaction
    /// </summary>
    private static bool ValidateAccount(CreateIncomeTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!request.CreditAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions must credit a tracked account"));
        }
        if (request.DebitAccount?.Id == request.CreditAccount.Id)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Debit and Credit Accounts cannot be the same"));
        }
        if (request.DebitAccount != null && request.DebitAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions cannot debit a tracked account"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the posted dates for this Income Transaction
    /// </summary>
    private static bool ValidatePostedDates(
        AccountingPeriod accountingPeriod,
        Account creditAccount,
        DateOnly? creditPostedDate,
        Account? debitAccount,
        DateOnly? debitPostedDate,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidatePostedDate(accountingPeriod, creditAccount, creditPostedDate, out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (debitAccount != null)
        {
            if (!ValidatePostedDate(accountingPeriod, debitAccount, debitPostedDate, out IEnumerable<Exception> debitPostedDateExceptions))
            {
                exceptions = exceptions.Concat(debitPostedDateExceptions);
            }
        }
        else if (debitPostedDate.HasValue)
        {
            exceptions = exceptions.Append(new InvalidDateException("A posted date cannot be provided for the debit account if no debit account is provided"));
        }
        return !exceptions.Any();
    }
}