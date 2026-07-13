using System.Diagnostics.CodeAnalysis;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Domain.Validation;

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
        out IEnumerable<ValidationError> exceptions)
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
                exceptions = [new ValidationError(ValidationErrorPath.Empty, $"Unrecognized create request type: {request.GetType().Name}")];
                transaction = null;
                return false;
        }
    }

    /// <summary>
    /// Attempts to update the provided transaction using the provided concrete request.
    /// </summary>
    public bool TryUpdate(Transaction transaction, UpdateTransactionRequest request, out IEnumerable<ValidationError> exceptions) =>
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
    public bool TryPost(Transaction transaction, PostTransactionRequest request, out IEnumerable<ValidationError> exceptions) =>
        transaction switch
        {
            SpendingTransaction spendingTransaction => spendingTransactionService.TryPost(spendingTransaction, request, out exceptions),
            IncomeTransaction incomeTransaction => incomeTransactionService.TryPost(incomeTransaction, request, out exceptions),
            AccountTransaction accountTransaction => accountTransactionService.TryPost(accountTransaction, request, out exceptions),
            FundTransaction => Fail(out exceptions, "Fund transactions cannot be posted to an account."),
            _ => Fail(out exceptions, $"Unrecognized transaction type: {transaction.GetType().Name}")
        };

    /// <summary>
    /// Attempts to unpost the provided transaction.
    /// </summary>
    public bool TryUnpost(Transaction transaction, out IEnumerable<ValidationError> exceptions) =>
        transaction switch
        {
            SpendingTransaction spendingTransaction => spendingTransactionService.TryUnpost(spendingTransaction, out exceptions),
            IncomeTransaction incomeTransaction => incomeTransactionService.TryUnpost(incomeTransaction, out exceptions),
            AccountTransaction accountTransaction => accountTransactionService.TryUnpost(accountTransaction, out exceptions),
            FundTransaction => Fail(out exceptions, "Fund transactions cannot be unposted."),
            _ => Fail(out exceptions, $"Unrecognized transaction type: {transaction.GetType().Name}")
        };

    /// <summary>
    /// Attempts to delete the provided transaction.
    /// </summary>
    public bool TryDelete(Transaction transaction, out IEnumerable<ValidationError> exceptions) =>
        transaction switch
        {
            SpendingTransaction spendingTransaction => spendingTransactionService.TryDelete(spendingTransaction, out exceptions),
            IncomeTransaction incomeTransaction => incomeTransactionService.TryDelete(incomeTransaction, out exceptions),
            AccountTransaction accountTransaction => accountTransactionService.TryDelete(accountTransaction, out exceptions),
            FundTransaction fundTransaction => fundTransactionService.TryDelete(fundTransaction, out exceptions),
            _ => Fail(out exceptions, $"Unrecognized transaction type: {transaction.GetType().Name}")
        };

    private static bool Fail(out IEnumerable<ValidationError> exceptions, string message)
    {
        exceptions = [new ValidationError(ValidationErrorPath.Empty, message)];
        return false;
    }
}