using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Refunds;
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
    FundTransactionService fundTransactionService,
    RefundTransactionService refundTransactionService)
{
    /// <summary>
    /// Attempts to create a transaction using the provided concrete request.
    /// </summary>
    public bool TryCreate(
        CreateTransactionRequest request,
        [NotNullWhen(true)] out Transaction? transaction,
        out IEnumerable<ValidationError> exceptions)
    {
        bool succeeded;
        (succeeded, transaction) = request switch
        {
            CreateSpendingTransactionRequest spendingRequest =>
                (spendingTransactionService.TryCreate(spendingRequest, out SpendingTransaction? spendingTransaction, out exceptions), (Transaction?)spendingTransaction),
            CreateIncomeTransactionRequest incomeRequest =>
                (incomeTransactionService.TryCreate(incomeRequest, out IncomeTransaction? incomeTransaction, out exceptions), incomeTransaction),
            CreateAccountTransactionRequest accountRequest =>
                (accountTransactionService.TryCreate(accountRequest, out AccountTransaction? accountTransaction, out exceptions), accountTransaction),
            CreateFundTransactionRequest fundRequest =>
                (fundTransactionService.TryCreate(fundRequest, out FundTransaction? fundTransaction, out exceptions), fundTransaction),
            CreateRefundTransactionRequest refundRequest =>
                (refundTransactionService.TryCreate(refundRequest, out RefundTransaction? refundTransaction, out exceptions), refundTransaction),
            _ => (Fail(out exceptions, $"Unrecognized create request type: {request.GetType().Name}"), null)
        };
        return succeeded;
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
            (RefundTransaction refundTransaction, UpdateRefundTransactionRequest refundRequest) =>
                refundTransactionService.TryUpdate(refundTransaction, refundRequest, out exceptions),
            _ => Fail(out exceptions, $"Request {request.GetType().Name} is not valid for transaction {transaction.GetType().Name}.")
        };

    /// <summary>
    /// Attempts to post the provided transaction to the provided account.
    /// </summary>
    public bool TryPost(Transaction transaction, PostTransactionRequest request, out IEnumerable<ValidationError> exceptions) =>
        transaction is FundTransaction
            ? Fail(out exceptions, "Fund transactions cannot be posted to an account.")
            : GetService(transaction)?.TryPost(transaction, request, out exceptions) ?? FailUnrecognized(transaction, out exceptions);

    /// <summary>
    /// Attempts to unpost the provided transaction.
    /// </summary>
    public bool TryUnpost(Transaction transaction, AccountId? accountId, out IEnumerable<ValidationError> exceptions) =>
        transaction is FundTransaction
            ? Fail(out exceptions, "Fund transactions cannot be unposted.")
            : GetService(transaction)?.TryUnpost(transaction, accountId, out exceptions) ?? FailUnrecognized(transaction, out exceptions);

    /// <summary>
    /// Attempts to delete the provided transaction.
    /// </summary>
    public bool TryDelete(Transaction transaction, out IEnumerable<ValidationError> exceptions) =>
        GetService(transaction)?.TryDelete(transaction, out exceptions) ?? FailUnrecognized(transaction, out exceptions);

    /// <summary>
    /// Gets the concrete transaction service for the provided transaction.
    /// </summary>
    private TransactionService? GetService(Transaction transaction) =>
        transaction switch
        {
            SpendingTransaction => spendingTransactionService,
            IncomeTransaction => incomeTransactionService,
            AccountTransaction => accountTransactionService,
            FundTransaction => fundTransactionService,
            RefundTransaction => refundTransactionService,
            _ => null
        };

    /// <summary>
    /// Helper method to fail for an unrecognized transaction type.
    /// </summary>
    private static bool FailUnrecognized(Transaction transaction, out IEnumerable<ValidationError> exceptions) =>
        Fail(out exceptions, $"Unrecognized transaction type: {transaction.GetType().Name}");

    /// <summary>
    /// Helper method to fail with a single exception message.
    /// </summary>
    private static bool Fail(out IEnumerable<ValidationError> exceptions, string message)
    {
        exceptions = [new ValidationError(ValidationErrorPath.Empty, message)];
        return false;
    }
}
