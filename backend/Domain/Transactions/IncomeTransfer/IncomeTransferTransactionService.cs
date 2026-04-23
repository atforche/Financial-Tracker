using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Transactions.Income;

namespace Domain.Transactions.IncomeTransfer;

/// <summary>
/// Service for managing Income Transfer Transactions
/// </summary>
public class IncomeTransferTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    FundGoalService fundGoalService,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository) :
    IncomeTransactionService(
        accountBalanceService, 
        accountingPeriodBalanceService, 
        fundBalanceService, 
        fundGoalService,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Income Transfer Transaction
    /// </summary>
    public bool TryCreate(
        CreateIncomeTransferTransactionRequest request,
        [NotNullWhen(true)] out IncomeTransferTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = transactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new IncomeTransferTransaction(request, sequence);
        AddTransaction(transaction);
        if (request.PostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.AccountId, request.PostedDate.Value, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        if (request.DebitPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out IEnumerable<Exception> postingExceptions))
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
    /// Attempts to update an existing Income Transfer Transaction
    /// </summary>
    public bool TryUpdate(
        IncomeTransferTransaction transaction,
        UpdateIncomeTransferTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUpdate(transaction, request, out exceptions))
        {
            return false;
        }
        transaction.UpdateFundAssignments(request.FundAssignments);
        UpdateTransaction(transaction, request);
        if (request.PostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.AccountId, request.PostedDate.Value, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        if (request.DebitPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Attempts to post an existing Income Transfer Transaction to a specific Account
    /// </summary>
    public bool TryPost(
        IncomeTransferTransaction transaction,
        AccountId accountId,
        DateOnly postedDate,
        out IEnumerable<Exception> exceptions)
    {
        if (!ValidatePosting(transaction, accountId, postedDate, out exceptions))
        {
            return false;
        }
        if (accountId == transaction.AccountId)
        {
            transaction.PostedDate = postedDate;
        }
        else if (accountId == transaction.DebitAccountId)
        {
            transaction.DebitPostedDate = postedDate;
        }
        PostTransaction(transaction, accountId);
        return true;
    }

    /// <summary>
    /// Attempts to unpost an existing Income Transfer Transaction
    /// </summary>
    public bool TryUnpost(IncomeTransferTransaction transaction, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUnposting(transaction, out exceptions))
        {
            return false;
        }
        transaction.PostedDate = null;
        transaction.DebitPostedDate = null;
        UnpostTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Income Transfer Transaction
    /// </summary>
    private bool ValidateCreate(CreateIncomeTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(request, [request.Account, request.DebitAccount], out exceptions);

        if (!ValidateDebitAccount(request, out IEnumerable<Exception> debitAccountExceptions))
        {
            exceptions = exceptions.Concat(debitAccountExceptions);
        }
        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(request.AccountingPeriodId);
        if (!ValidatePostedDate(accountingPeriod, request.DebitAccount, request.DebitPostedDate, out IEnumerable<Exception> debitPostedDateExceptions))
        {
            exceptions = exceptions.Concat(debitPostedDateExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to update an existing Income Transfer Transaction
    /// </summary>
    private bool ValidateUpdate(
        IncomeTransferTransaction transaction,
        UpdateIncomeTransferTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        Account account = accountRepository.GetById(transaction.AccountId);
        Account debitAccount = accountRepository.GetById(transaction.DebitAccountId);
        _ = ValidateUpdate(transaction, request, [account, debitAccount], out exceptions);

        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        if (!ValidatePostedDate(accountingPeriod, debitAccount, transaction.DebitPostedDate, out IEnumerable<Exception> debitPostedDateExceptions))
        {
            exceptions = exceptions.Concat(debitPostedDateExceptions);
        }
        if (transaction.DebitPostedDate.HasValue)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction has already been posted and cannot be updated"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Debit Account for this Income Transfer Transaction
    /// </summary>
    private static bool ValidateDebitAccount(CreateIncomeTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.DebitAccount.Id == request.Account.Id)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Debit and Credit Accounts cannot be the same"));
        }
        if (request.DebitAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Debit Account must be an untracked account"));
        }
        return !exceptions.Any();
    }
}