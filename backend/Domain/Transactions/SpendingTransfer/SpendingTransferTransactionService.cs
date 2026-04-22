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
    /// Validates a request to create a new Spending Transfer Transaction
    /// </summary>
    private bool ValidateCreate(CreateSpendingTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(request, [request.Account, request.CreditAccount], out exceptions);

        if (!ValidateCreditAccount(request, out IEnumerable<Exception> creditAccountExceptions))
        {
            exceptions = exceptions.Concat(creditAccountExceptions);
        }
        if (!ValidateCreditPostedDate(request, out IEnumerable<Exception> creditPostedDateExceptions))
        {
            exceptions = exceptions.Concat(creditPostedDateExceptions);
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

    /// <summary>
    /// Validates the Credit Posted Date for this Spending Transfer Transaction
    /// </summary>
    private bool ValidateCreditPostedDate(CreateSpendingTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.CreditPostedDate.HasValue)
        {
            if (!ValidatePostedDate(request, request.CreditAccount, request.CreditPostedDate.Value, out IEnumerable<Exception> creditPostedDateExceptions))
            {
                exceptions = exceptions.Concat(creditPostedDateExceptions);
            }
        }
        return !exceptions.Any();
    }
}