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
    FundGoalService fundGoalService,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IFundRepository fundRepository,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService, 
        accountingPeriodBalanceService, 
        fundBalanceService, 
        fundGoalService,
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

        if (!ValidateCreate(request, [request.Account], out exceptions))
        {
            return false;
        }
        int sequence = transactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new IncomeTransaction(request, sequence);
        AddTransaction(transaction);
        if (request.PostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.AccountId, request.PostedDate.Value, out exceptions))
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
        Account account = accountRepository.GetById(transaction.AccountId);
        if (!ValidateUpdate(transaction, request, [account], out exceptions))
        {
            return false;
        }
        transaction.UpdateFundAssignments(request.FundAssignments);
        UpdateTransaction(transaction, request);
        if (request.PostedDate.HasValue)
        {
            if (!TryPost(transaction, transaction.AccountId, request.PostedDate.Value, out exceptions))
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
        transaction.PostedDate = postedDate;
        PostTransaction(transaction, accountId);
        return true;
    }

    /// <summary>
    /// Attempts to unpost an existing Income Transaction
    /// </summary>
    public bool TryUnpost(IncomeTransaction transaction, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUnposting(transaction, out exceptions))
        {
            return false;
        }
        transaction.PostedDate = null;
        UnpostTransaction(transaction);
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
        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(request.AccountingPeriodId);
        if (!ValidatePostedDate(accountingPeriod, request.Account, request.PostedDate, out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (!ValidateFundAssignments(request.Amount, request.FundAssignments, out IEnumerable<Exception> fundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(fundAssignmentExceptions);
        }
        if (!ValidateInitialTransactionForAccount(request.IsInitialTransactionForAccount, request.Account, out IEnumerable<Exception> initialTransactionForAccountExceptions))
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
        _ = ValidateUpdate(transaction, request, accounts, out exceptions);

        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        Account account = accountRepository.GetById(transaction.AccountId);
        if (transaction.GeneratedByAccountId != null)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction was auto-generated by an account and cannot be updated directly"));
        }
        if (!ValidatePostedDate(accountingPeriod, account, request.PostedDate, out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (transaction.PostedDate.HasValue)
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
    protected bool ValidateUnposting(IncomeTransaction transaction, out IEnumerable<Exception> exceptions)
    {
        _ = base.ValidateUnposting(transaction, out exceptions);

        if (transaction.GeneratedByAccountId != null)
        {
            exceptions = exceptions.Append(new UnableToUnpostException("Transaction was auto-generated by an account and cannot be unposted"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Account for this Income Transaction
    /// </summary>
    private static bool ValidateAccount(CreateIncomeTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!request.Account.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions must credit a tracked account"));
        }
        return !exceptions.Any();
    }
}