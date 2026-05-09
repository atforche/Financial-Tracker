using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;

namespace Domain.Transactions;

/// <summary>
/// Dispatches transaction commands to the correct concrete transaction service.
/// </summary>
public sealed class TransactionDispatcherService(
    SpendingTransactionService spendingTransactionService,
    IncomeTransactionService incomeTransactionService,
    AccountTransactionService accountTransactionService,
    FundTransactionService fundTransactionService)
{
    /// <summary>
    /// Attempts to create a transaction using the provided concrete request.
    /// </summary>
    public bool TryCreate(
        CreateTransactionRequest request,
        [NotNullWhen(true)] out Transaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        switch (request)
        {
            case CreateSpendingTransactionRequest spendingRequest:
                if (spendingTransactionService.TryCreate(spendingRequest, out SpendingTransaction? spendingTransaction, out exceptions))
                {
                    transaction = spendingTransaction;
                    return true;
                }
                transaction = null;
                return false;
            case CreateIncomeTransactionRequest incomeRequest:
                if (incomeTransactionService.TryCreate(incomeRequest, out IncomeTransaction? incomeTransaction, out exceptions))
                {
                    transaction = incomeTransaction;
                    return true;
                }
                transaction = null;
                return false;
            case CreateAccountTransactionRequest accountRequest:
                if (accountTransactionService.TryCreate(accountRequest, out AccountTransaction? accountTransaction, out exceptions))
                {
                    transaction = accountTransaction;
                    return true;
                }
                transaction = null;
                return false;
            case CreateFundTransactionRequest fundRequest:
                if (fundTransactionService.TryCreate(fundRequest, out FundTransaction? fundTransaction, out exceptions))
                {
                    transaction = fundTransaction;
                    return true;
                }
                transaction = null;
                return false;
            default:
                exceptions = [new InvalidOperationException($"Unrecognized create request type: {request.GetType().Name}")];
                transaction = null;
                return false;
        }
    }

    /// <summary>
    /// Attempts to update the provided transaction using the provided concrete request.
    /// </summary>
    public bool TryUpdate(Transaction transaction, UpdateTransactionRequest request, out IEnumerable<Exception> exceptions) =>
        (transaction, request) switch
        {
            (SpendingTransaction spendingTransaction, UpdateSpendingTransactionRequest spendingRequest) =>
                spendingTransactionService.TryUpdate(spendingTransaction, spendingRequest, out exceptions),
            (IncomeTransaction incomeTransaction, UpdateIncomeTransactionRequest incomeRequest) =>
                incomeTransactionService.TryUpdate(incomeTransaction, incomeRequest, out exceptions),
            (AccountTransaction accountTransaction, UpdateAccountTransactionRequest accountRequest) =>
                accountTransactionService.TryUpdate(accountTransaction, accountRequest, out exceptions),
            (FundTransaction fundTransaction, UpdateFundTransactionRequest fundRequest) =>
                fundTransactionService.TryUpdate(fundTransaction, fundRequest, out exceptions),
            _ => Fail(out exceptions, $"Request {request.GetType().Name} is not valid for transaction {transaction.GetType().Name}.")
        };

    /// <summary>
    /// Attempts to post the provided transaction to the provided account.
    /// </summary>
    public bool TryPost(Transaction transaction, AccountId accountId, DateOnly postedDate, out IEnumerable<Exception> exceptions) =>
        transaction switch
        {
            SpendingTransaction spendingTransaction => spendingTransactionService.TryPost(spendingTransaction, accountId, postedDate, out exceptions),
            IncomeTransaction incomeTransaction => incomeTransactionService.TryPost(incomeTransaction, accountId, postedDate, out exceptions),
            AccountTransaction accountTransaction => accountTransactionService.TryPost(accountTransaction, accountId, postedDate, out exceptions),
            FundTransaction => Fail(out exceptions, "Fund transactions cannot be posted to an account.", exceptionFactory: message => new UnableToPostException(message)),
            _ => Fail(out exceptions, $"Unrecognized transaction type: {transaction.GetType().Name}")
        };

    /// <summary>
    /// Attempts to unpost the provided transaction.
    /// </summary>
    public bool TryUnpost(Transaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions) =>
        transaction switch
        {
            SpendingTransaction spendingTransaction => spendingTransactionService.TryUnpost(spendingTransaction, accountBeingDeleted, out exceptions),
            IncomeTransaction incomeTransaction => incomeTransactionService.TryUnpost(incomeTransaction, accountBeingDeleted, out exceptions),
            AccountTransaction accountTransaction => accountTransactionService.TryUnpost(accountTransaction, accountBeingDeleted, out exceptions),
            FundTransaction => Fail(out exceptions, "Fund transactions cannot be unposted.", exceptionFactory: message => new UnableToUnpostException(message)),
            _ => Fail(out exceptions, $"Unrecognized transaction type: {transaction.GetType().Name}")
        };

    /// <summary>
    /// Attempts to delete the provided transaction.
    /// </summary>
    public bool TryDelete(Transaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions) =>
        transaction switch
        {
            SpendingTransaction spendingTransaction => spendingTransactionService.TryDelete(spendingTransaction, accountBeingDeleted, out exceptions),
            IncomeTransaction incomeTransaction => incomeTransactionService.TryDelete(incomeTransaction, accountBeingDeleted, out exceptions),
            AccountTransaction accountTransaction => accountTransactionService.TryDelete(accountTransaction, accountBeingDeleted, out exceptions),
            FundTransaction fundTransaction => fundTransactionService.TryDelete(fundTransaction, out exceptions),
            _ => Fail(out exceptions, $"Unrecognized transaction type: {transaction.GetType().Name}", exceptionFactory: message => new UnableToDeleteException(message))
        };

    private static bool Fail(out IEnumerable<Exception> exceptions, string message, Func<string, Exception>? exceptionFactory = null)
    {
        exceptions = [exceptionFactory?.Invoke(message) ?? new InvalidOperationException(message)];
        return false;
    }
}