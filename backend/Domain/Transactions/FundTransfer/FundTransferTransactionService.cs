using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Transactions.FundTransfer;

/// <summary>
/// Service for managing Fund Transfer Transactions
/// </summary>
public class FundTransferTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    FundGoalService fundGoalService,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService, 
        accountingPeriodBalanceService, 
        fundBalanceService, 
        fundGoalService,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Fund Transfer Transaction
    /// </summary>
    public bool TryCreate(
        CreateFundTransferTransactionRequest request,
        [NotNullWhen(true)] out FundTransferTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = transactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new FundTransferTransaction(request, sequence);
        AddTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Fund Transfer Transaction
    /// </summary>
    private bool ValidateCreate(CreateFundTransferTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(request, [], [request.DebitFund, request.CreditFund], out exceptions);
        if (request.DebitFund == request.CreditFund)
        {
            exceptions = exceptions.Append(new InvalidFundException("Debit and Credit Funds cannot be the same"));
        }
        return !exceptions.Any();
    }
}