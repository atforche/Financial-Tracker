using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Transactions.Funds;

/// <summary>
/// Service for managing Fund Transactions
/// </summary>
public class FundTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService,
        accountingPeriodBalanceService,
        fundBalanceService,
        accountingPeriodRepository,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Fund Transfer Transaction
    /// </summary>
    public bool TryCreate(
        CreateFundTransactionRequest request,
        [NotNullWhen(true)] out FundTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new FundTransaction(request, sequence);
        AddTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund Transaction
    /// </summary>
    public bool TryUpdate(
        FundTransaction transaction,
        UpdateFundTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUpdate(transaction, request, [], out exceptions))
        {
            return false;
        }
        UpdateTransaction(transaction, request);
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Fund Transaction
    /// </summary>
    public bool TryDelete(FundTransaction transaction, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateDelete(transaction, out exceptions))
        {
            return false;
        }
        DeleteTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Fund Transaction
    /// </summary>
    private bool ValidateCreate(CreateFundTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(request, [], [request.DebitFund, request.CreditFund], out exceptions);
        if (request.DebitFund == request.CreditFund)
        {
            exceptions = exceptions.Append(new InvalidFundException("Debit and Credit Funds cannot be the same"));
        }
        return !exceptions.Any();
    }
}