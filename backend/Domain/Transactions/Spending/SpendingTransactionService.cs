using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Transactions.Spending;

/// <summary>
/// Service for managing Spending Transactions
/// </summary>
public class SpendingTransactionService(
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
    /// Attempts to create a new Spending Transaction
    /// </summary>
    public bool TryCreate(
        CreateSpendingTransactionRequest request,
        [NotNullWhen(true)] out SpendingTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(
                request,
                new List<Account?> { request.DebitAccount, request.CreditAccount }.OfType<Account>().ToList(),
                out exceptions))
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new SpendingTransaction(request, sequence);
        AddTransaction(transaction);
        if (request.DebitPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out exceptions))
            {
                transaction = null;
                return false;
            }
        }
        if (request.CreditPostedDate.HasValue && transaction.CreditAccountId != null)
        {
            if (!TryPost(transaction, transaction.CreditAccountId, request.CreditPostedDate.Value, out exceptions))
            {
                transaction = null;
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Spending Transaction
    /// </summary>
    public bool TryUpdate(
        SpendingTransaction transaction,
        UpdateSpendingTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        Account debitAccount = accountRepository.GetById(transaction.DebitAccountId);
        Account? creditAccount = transaction.CreditAccountId != null ? accountRepository.GetById(transaction.CreditAccountId) : null;
        if (!ValidateUpdate(
                transaction,
                request,
                new List<Account?> { debitAccount, creditAccount }.OfType<Account>().ToList(),
                out exceptions))
        {
            return false;
        }
        transaction.UpdateFundAssignments(request.FundAssignments);
        UpdateTransaction(transaction, request);
        if (request.DebitPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out exceptions))
            {
                return false;
            }
        }
        if (request.CreditPostedDate.HasValue && transaction.CreditAccountId != null)
        {
            if (!TryPost(transaction, transaction.CreditAccountId, request.CreditPostedDate.Value, out exceptions))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Spending Transaction to a specific Account
    /// </summary>
    public bool TryPost(
        SpendingTransaction transaction,
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
    /// Attempts to unpost an existing Spending Transaction
    /// </summary>
    public bool TryUnpost(SpendingTransaction transaction, out IEnumerable<Exception> exceptions)
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
    /// Attempts to delete an existing Spending Transaction
    /// </summary>
    public bool TryDelete(SpendingTransaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateDelete(transaction, accountBeingDeleted, out exceptions))
        {
            return false;
        }
        DeleteTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Spending Transaction
    /// </summary>
    protected bool ValidateCreate(
        CreateSpendingTransactionRequest request,
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
                request.DebitAccount,
                request.DebitPostedDate,
                request.CreditAccount,
                request.CreditPostedDate,
                out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (!ValidateFundAssignments(request.Amount, request.FundAssignments, out IEnumerable<Exception> fundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(fundAssignmentExceptions);
        }
        if (!ValidateInitialTransactionForAccount(request.IsInitialTransactionForAccount, request.DebitAccount, out IEnumerable<Exception> initialTransactionForAccountExceptions))
        {
            exceptions = exceptions.Concat(initialTransactionForAccountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to update an existing Spending Transaction
    /// </summary>
    protected bool ValidateUpdate(
        SpendingTransaction transaction,
        UpdateSpendingTransactionRequest request,
        IReadOnlyCollection<Account> accounts,
        out IEnumerable<Exception> exceptions)
    {
        _ = base.ValidateUpdate(transaction, request, accounts, out exceptions);

        AccountingPeriod accountingPeriod = AccountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        Account debitAccount = accountRepository.GetById(transaction.DebitAccountId);
        Account? creditAccount = transaction.CreditAccountId != null ? accountRepository.GetById(transaction.CreditAccountId) : null;
        if (transaction.GeneratedByAccountId != null)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction was auto-generated by an account and cannot be updated directly"));
        }
        if (!ValidatePostedDates(
                accountingPeriod,
                debitAccount,
                request.DebitPostedDate ?? transaction.DebitPostedDate,
                creditAccount,
                request.CreditPostedDate ?? transaction.CreditPostedDate,
                out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (transaction.DebitPostedDate.HasValue || transaction.CreditPostedDate.HasValue)
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
    /// Validates a request to delete an existing Spending Transaction
    /// </summary>
    protected bool ValidateDelete(SpendingTransaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateDelete(transaction, out exceptions);

        if (transaction.GeneratedByAccountId != null && transaction.GeneratedByAccountId != accountBeingDeleted)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Transaction was auto-generated by an account and cannot be deleted directly"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Fund Assignments for this Spending Transaction
    /// </summary>
    protected override bool ValidateFundAssignments(
        decimal amount,
        IReadOnlyCollection<FundAmount> fundAssignments,
        out IEnumerable<Exception> exceptions)
    {
        _ = base.ValidateFundAssignments(amount, fundAssignments, out exceptions);

        Fund? unassignedFund = fundRepository.GetSystemFund();
        if (fundAssignments.Any(fundAmount => fundAmount.FundId == unassignedFund?.Id))
        {
            exceptions = exceptions.Append(new InvalidFundAmountException("Cannot spend money from the unassigned fund"));
        }
        if (fundAssignments.Sum(fundAmount => fundAmount.Amount) != amount)
        {
            exceptions = exceptions.Append(new InvalidFundAmountException("Total amount assigned to funds must equal the transaction amount"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to unpost an existing Spending Transaction
    /// </summary>
    protected bool ValidateUnposting(SpendingTransaction transaction, out IEnumerable<Exception> exceptions)
    {
        _ = base.ValidateUnposting(transaction, out exceptions);

        if (transaction.GeneratedByAccountId != null)
        {
            exceptions = exceptions.Append(new UnableToUnpostException("Transaction was auto-generated by an account and cannot be unposted"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Account for this Spending Transaction
    /// </summary>
    private static bool ValidateAccount(CreateSpendingTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!request.DebitAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions must debit a tracked account"));
        }
        if (request.CreditAccount?.Id == request.DebitAccount.Id)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Debit and Credit Accounts cannot be the same"));
        }
        if (request.CreditAccount != null && request.CreditAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions cannot credit a tracked account"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the posted dates for this Spending Transaction
    /// </summary>
    private static bool ValidatePostedDates(
        AccountingPeriod accountingPeriod,
        Account debitAccount,
        DateOnly? debitPostedDate,
        Account? creditAccount,
        DateOnly? creditPostedDate,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidatePostedDate(accountingPeriod, debitAccount, debitPostedDate, out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (creditAccount != null)
        {
            if (!ValidatePostedDate(accountingPeriod, creditAccount, creditPostedDate, out IEnumerable<Exception> creditPostedDateExceptions))
            {
                exceptions = exceptions.Concat(creditPostedDateExceptions);
            }
        }
        else if (creditPostedDate.HasValue)
        {
            exceptions = exceptions.Append(new InvalidDateException("A posted date cannot be provided for the credit account if no credit account is provided"));
        }
        return !exceptions.Any();
    }
}