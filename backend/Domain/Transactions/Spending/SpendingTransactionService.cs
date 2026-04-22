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
    FundGoalService fundGoalService,
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
    /// Attempts to create a new Spending Transaction
    /// </summary>
    public bool TryCreate(
        CreateSpendingTransactionRequest request,
        [NotNullWhen(true)] out SpendingTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, [request.Account], out exceptions))
        {
            return false;
        }
        int sequence = transactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new SpendingTransaction(request, sequence);
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
        if (!ValidatePostedDates(request, out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
        }
        if (!ValidateFundAssignments(request, out IEnumerable<Exception> fundAssignmentExceptions))
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
    /// Validates the Account for this Spending Transaction
    /// </summary>
    private static bool ValidateAccount(CreateSpendingTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];
        if (!request.Account.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions must debit a tracked account"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Posted Date for this Spending Transaction
    /// </summary>
    private bool ValidatePostedDates(CreateSpendingTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.PostedDate.HasValue)
        {
            if (!ValidatePostedDate(request, request.Account, request.PostedDate.Value, out IEnumerable<Exception> postedDateExceptions))
            {
                exceptions = exceptions.Concat(postedDateExceptions);
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Fund Assignments for this Spending Transaction
    /// </summary>
    private bool ValidateFundAssignments(CreateSpendingTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateFundAssignments(request, request.FundAssignments, out exceptions);

        Fund? unassignedFund = fundRepository.GetSystemFund();
        if (request.FundAssignments.Any(fundAmount => fundAmount.FundId == unassignedFund?.Id))
        {
            exceptions = exceptions.Append(new InvalidFundAmountException("Cannot spend money from the unassigned fund"));
        }
        return !exceptions.Any();
    }
}