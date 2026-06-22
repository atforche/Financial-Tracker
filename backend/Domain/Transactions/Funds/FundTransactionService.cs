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
        if (!ValidateUpdate(transaction, request, out exceptions))
        {
            return false;
        }
        UpdateTransaction(
            transaction,
            request,
            () =>
            {
                transaction.UpdateFundSource(request.Source);
                transaction.UpdateFundDestinations(request.Destinations);
            });
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
        _ = ValidateCreate(request, [], GetFunds(request), out exceptions);
        if (!ValidateFunds(request.Source, request.Destinations, out IEnumerable<Exception> fundExceptions))
        {
            exceptions = exceptions.Concat(fundExceptions);
        }
        if (!ValidateAmounts(request.Amount, request.Destinations, out IEnumerable<Exception> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to update an existing Fund Transaction
    /// </summary>
    private bool ValidateUpdate(
        FundTransaction transaction,
        UpdateFundTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        _ = ValidateUpdate(transaction, request, [], GetFunds(request), out exceptions);

        if (!ValidateFunds(request.Source, request.Destinations, out IEnumerable<Exception> fundExceptions))
        {
            exceptions = exceptions.Concat(fundExceptions);
        }
        if (!ValidateAmounts(request.Amount, request.Destinations, out IEnumerable<Exception> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the fund structure for this Fund Transaction
    /// </summary>
    private static bool ValidateFunds(
        FundTransactionSource source,
        IReadOnlyCollection<FundTransactionDestination> destinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Fund Transactions must have at least one destination fund"));
        }
        foreach (FundTransactionDestination destination in destinations)
        {
            if (destination.Fund.Id == source.Fund.Id)
            {
                exceptions = exceptions.Append(new InvalidFundException("Source and destination funds cannot be the same"));
            }
        }
        var destinationFundIds = destinations
            .Select(destination => destination.Fund.Id)
            .ToList();
        if (destinationFundIds.Distinct().Count() != destinationFundIds.Count)
        {
            exceptions = exceptions.Append(new InvalidFundException("Duplicate destination funds are not allowed"));
        }
        return !exceptions.Any();
    }

    private static bool ValidateAmounts(
        decimal amount,
        IReadOnlyCollection<FundTransactionDestination> destinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Fund Transactions must have at least one destination fund"));
        }
        if (destinations.Any(destination => destination.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Fund destination amounts must be positive"));
        }
        if (Math.Round(destinations.Sum(destination => destination.Amount), 2) != Math.Round(amount, 2))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Fund destination amounts must equal the transaction amount"));
        }
        return !exceptions.Any();
    }

    private static List<Fund> GetFunds(CreateFundTransactionRequest request) =>
        [request.Source.Fund, .. request.Destinations.Select(destination => destination.Fund)];

    private static List<Fund> GetFunds(UpdateFundTransactionRequest request) =>
        [request.Source.Fund, .. request.Destinations.Select(destination => destination.Fund)];
}