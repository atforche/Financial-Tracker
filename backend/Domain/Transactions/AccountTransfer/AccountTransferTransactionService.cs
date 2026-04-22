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
    /// Validates a request to create a new Account Transfer Transaction
    /// </summary>
    private bool ValidateCreate(CreateAccountTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(request, [request.DebitAccount, request.CreditAccount], [], out exceptions);
        if (!ValidateAccounts(request, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidatePostedDates(request, out IEnumerable<Exception> postedDateExceptions))
        {
            exceptions = exceptions.Concat(postedDateExceptions);
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
    private bool ValidatePostedDates(CreateAccountTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.DebitPostedDate.HasValue)
        {
            if (!ValidatePostedDate(request, request.DebitAccount, request.DebitPostedDate.Value, out IEnumerable<Exception> debitPostedDateExceptions))
            {
                exceptions = exceptions.Concat(debitPostedDateExceptions);
            }
        }
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