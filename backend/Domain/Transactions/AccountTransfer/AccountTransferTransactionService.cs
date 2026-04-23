using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Transactions.AccountTransfer;

/// <summary>
/// Service for managing Account Transfer Transactions
/// </summary>
public class AccountTransferTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    FundGoalService fundGoalService,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService, 
        accountingPeriodBalanceService, 
        fundBalanceService, 
        fundGoalService,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Account Transfer Transaction
    /// </summary>
    public bool TryCreate(
        CreateAccountTransferTransactionRequest request,
        [NotNullWhen(true)] out AccountTransferTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = transactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new AccountTransferTransaction(request, sequence);
        AddTransaction(transaction);
        if (request.DebitPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out IEnumerable<Exception> postingExceptions))
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
    /// Attempts to update an existing Account Transfer Transaction
    /// </summary>
    public bool TryUpdate(
        AccountTransferTransaction transaction,
        UpdateAccountTransferTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUpdate(transaction, request, out exceptions))
        {
            return false;
        }
        UpdateTransaction(transaction, request);
        if (request.DebitPostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.DebitAccountId, request.DebitPostedDate.Value, out IEnumerable<Exception> postingExceptions))
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
        return !exceptions.Any();
    }

    /// <summary>
    /// Attempts to post an existing Account Transfer Transaction to a specific Account
    /// </summary>
    public bool TryPost(
        AccountTransferTransaction transaction,
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
    /// Validates a request to create a new Account Transfer Transaction
    /// </summary>
    private bool ValidateCreate(CreateAccountTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(request, [request.DebitAccount, request.CreditAccount], [], out exceptions);

        if (!ValidateAccounts(request, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(request.AccountingPeriodId);
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
    /// Validates a request to update an existing Account Transfer Transaction
    /// </summary>
    private bool ValidateUpdate(
        AccountTransferTransaction transaction,
        UpdateAccountTransferTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        Account debitAccount = accountRepository.GetById(transaction.DebitAccountId);
        Account creditAccount = accountRepository.GetById(transaction.CreditAccountId);
        _ = ValidateUpdate(transaction, request, [debitAccount, creditAccount], out exceptions);

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
    /// Validates the Accounts for this Account Transfer Transaction
    /// </summary>
    private static bool ValidateAccounts(CreateAccountTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.DebitAccount.Id == request.CreditAccount.Id)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Debit and Credit Accounts must be different"));
        }
        if (!request.DebitAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Debit Account must be a tracked account"));
        }
        if (!request.CreditAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Credit Account must be a tracked account"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Posted Dates for this Account Transfer Transaction
    /// </summary>
    private static bool ValidatePostedDates(
        AccountingPeriod accountingPeriod,
        Account debitAccount,
        DateOnly? debitPostedDate,
        Account creditAccount,
        DateOnly? creditPostedDate,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidatePostedDate(accountingPeriod, debitAccount, debitPostedDate, out IEnumerable<Exception> debitPostedDateExceptions))
        {
            exceptions = exceptions.Concat(debitPostedDateExceptions);
        }
        if (!ValidatePostedDate(accountingPeriod, creditAccount, creditPostedDate, out IEnumerable<Exception> creditPostedDateExceptions))
        {
            exceptions = exceptions.Concat(creditPostedDateExceptions);
        }
        return !exceptions.Any();
    }
}