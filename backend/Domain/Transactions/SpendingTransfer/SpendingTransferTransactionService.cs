using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Transactions.Spending;

namespace Domain.Transactions.SpendingTransfer;

/// <summary>
/// Service for managing Spending Transfer Transactions
/// </summary>
public class SpendingTransferTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    FundGoalService fundGoalService,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository) :
    SpendingTransactionService(
        accountBalanceService, 
        accountingPeriodBalanceService, 
        fundBalanceService, 
        fundGoalService,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Spending Transfer Transaction
    /// </summary>
    public bool TryCreate(
        CreateSpendingTransferTransactionRequest request,
        [NotNullWhen(true)] out SpendingTransferTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = transactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new SpendingTransferTransaction(request, sequence);
        AddTransaction(transaction);
        if (request.PostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.AccountId, request.PostedDate.Value, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        if (request.CreditPostedDate.HasValue)
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
    /// Attempts to update an existing Spending Transfer Transaction
    /// </summary>
    public bool TryUpdate(
        SpendingTransferTransaction transaction,
        UpdateSpendingTransferTransactionRequest request,
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
        if (request.CreditPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.CreditAccountId, request.CreditPostedDate.Value, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        if (exceptions.Any())
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Spending Transfer Transaction to a specific Account
    /// </summary>
    public bool TryPost(
        SpendingTransferTransaction transaction,
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
        else if (accountId == transaction.CreditAccountId)
        {
            transaction.CreditPostedDate = postedDate;
        }
        PostTransaction(transaction, accountId);
        return true;
    }

    /// <summary>
    /// Attempts to unpost an existing Spending Transfer Transaction
    /// </summary>
    public bool TryUnpost(SpendingTransferTransaction transaction, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUnposting(transaction, out exceptions))
        {
            return false;
        }
        transaction.PostedDate = null;
        transaction.CreditPostedDate = null;
        UnpostTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Spending Transfer Transaction
    /// </summary>
    private bool ValidateCreate(CreateSpendingTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(request, [request.Account, request.CreditAccount], out exceptions);

        if (!ValidateCreditAccount(request, out IEnumerable<Exception> creditAccountExceptions))
        {
            exceptions = exceptions.Concat(creditAccountExceptions);
        }
        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(request.AccountingPeriodId);
        if (!ValidatePostedDate(accountingPeriod, request.CreditAccount, request.CreditPostedDate, out IEnumerable<Exception> creditPostedDateExceptions))
        {
            exceptions = exceptions.Concat(creditPostedDateExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to update an existing Spending Transfer Transaction
    /// </summary>
    private bool ValidateUpdate(SpendingTransferTransaction transaction, UpdateSpendingTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        Account account = accountRepository.GetById(transaction.AccountId);
        Account creditAccount = accountRepository.GetById(transaction.CreditAccountId);
        _ = ValidateUpdate(transaction, request, [account, creditAccount], out exceptions);

        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        if (!ValidatePostedDate(accountingPeriod, creditAccount, request.CreditPostedDate, out IEnumerable<Exception> creditPostedDateExceptions))
        {
            exceptions = exceptions.Concat(creditPostedDateExceptions);
        }
        if (transaction.CreditPostedDate.HasValue)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction has already been posted and cannot be updated"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Credit Account for this Spending Transfer Transaction
    /// </summary>
    private static bool ValidateCreditAccount(CreateSpendingTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.CreditAccount.Id == request.Account.Id)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Debit and Credit Accounts cannot be the same"));
        }
        if (request.CreditAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Credit Account must be an untracked account"));
        }
        return !exceptions.Any();
    }
}