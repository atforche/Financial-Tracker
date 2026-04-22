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
    /// Validates a request to create a new Income Transfer Transaction
    /// </summary>
    private bool ValidateCreate(CreateIncomeTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(request, [request.Account, request.DebitAccount], out exceptions);

        if (!ValidateDebitAccount(request, out IEnumerable<Exception> debitAccountExceptions))
        {
            exceptions = exceptions.Concat(debitAccountExceptions);
        }
        if (!ValidateDebitPostedDate(request, out IEnumerable<Exception> debitPostedDateExceptions))
        {
            exceptions = exceptions.Concat(debitPostedDateExceptions);
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

    /// <summary>
    /// Validates the Debit Posted Date for this Income Transfer Transaction
    /// </summary>
    private bool ValidateDebitPostedDate(CreateIncomeTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.DebitPostedDate.HasValue)
        {
            if (!ValidatePostedDate(request, request.DebitAccount, request.DebitPostedDate.Value, out IEnumerable<Exception> debitPostedDateExceptions))
            {
                exceptions = exceptions.Concat(debitPostedDateExceptions);
            }
        }
        return !exceptions.Any();
    }
}