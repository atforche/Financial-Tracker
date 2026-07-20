using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Validation;

namespace Domain.Transactions.Funds;

/// <summary>
/// Service for managing Fund Transactions
/// </summary>
public class FundTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    FundPlanTotalsHistoryService fundPlanTotalsHistoryService,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService,
        accountingPeriodBalanceService,
        fundBalanceService,
        fundPlanTotalsHistoryService,
        accountingPeriodRepository,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Fund Transfer Transaction
    /// </summary>
    public bool TryCreate(
        CreateFundTransactionRequest request,
        [NotNullWhen(true)] out FundTransaction? transaction,
        out IEnumerable<ValidationError> exceptions)
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
        out IEnumerable<ValidationError> exceptions)
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
    public bool TryDelete(FundTransaction transaction, out IEnumerable<ValidationError> exceptions)
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
    private bool ValidateCreate(CreateFundTransactionRequest request, out IEnumerable<ValidationError> exceptions)
    {
        _ = ValidateCreate(
            request,
            null,
            ValidationErrorPath.Empty,
            [],
            (i) => ValidationErrorPath.Empty,
            [request.Source.Fund],
            (i) => new ValidationErrorPath(nameof(CreateFundTransactionRequest.Source)).Append(nameof(FundTransactionSource.Fund)),
            request.Destinations.Select(destination => new List<Fund> { destination.Fund }).ToList(),
            (i, j) => new ValidationErrorPath(nameof(CreateFundTransactionRequest.Destinations), i).Append(nameof(FundTransactionDestination.Fund)),
            out exceptions);
        if (!ValidateFunds(
                request.Source,
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(CreateFundTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> fundExceptions))
        {
            exceptions = exceptions.Concat(fundExceptions);
        }
        if (!ValidateAmounts(
                request.Amount,
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(CreateFundTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> amountExceptions))
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
        out IEnumerable<ValidationError> exceptions)
    {
        _ = ValidateUpdate(
            transaction,
            request,
            null,
            ValidationErrorPath.Empty,
            [],
            (i) => ValidationErrorPath.Empty,
            [request.Source.Fund],
            (i) => new ValidationErrorPath(nameof(UpdateFundTransactionRequest.Source)).Append(nameof(FundTransactionSource.Fund)),
            request.Destinations.Select(destination => new List<Fund> { destination.Fund }).ToList(),
            (i, j) => new ValidationErrorPath(nameof(UpdateFundTransactionRequest.Destinations), i).Append(nameof(FundTransactionDestination.Fund)),
            out exceptions);

        if (!ValidateFunds(
                request.Source,
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(UpdateFundTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> fundExceptions))
        {
            exceptions = exceptions.Concat(fundExceptions);
        }
        if (!ValidateAmounts(
                request.Amount,
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(UpdateFundTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> amountExceptions))
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
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(0), "Fund Transactions must have at least one destination fund"));
        }
        foreach ((int index, FundTransactionDestination destination) in destinations.Index())
        {
            if (destination.Fund.Id == source.Fund.Id)
            {
                exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(index).Append(nameof(FundTransactionDestination.Fund)), "Source and destination funds cannot be the same"));
            }
            if (destinations.Index().Any(pair => pair.Item.Fund == destination.Fund && pair.Index != index))
            {
                exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(index).Append(nameof(FundTransactionDestination.Fund)), "Duplicate destination funds are not allowed"));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the amounts for this Fund Transaction
    /// </summary>
    private static bool ValidateAmounts(
        decimal amount,
        IReadOnlyCollection<FundTransactionDestination> destinations,
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(0), "Fund Transactions must have at least one destination fund"));
        }
        foreach ((int index, FundTransactionDestination destination) in destinations.Index())
        {
            if (destination.Amount <= 0)
            {
                exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(index).Append(nameof(FundTransactionDestination.Amount)), "Fund destination amounts must be positive"));
            }
            if (Math.Round(destinations.Sum(destination => destination.Amount), 2) != Math.Round(amount, 2))
            {
                exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(index).Append(nameof(FundTransactionDestination.Amount)), "Fund destination amounts must equal the transaction amount"));
            }
        }
        return !exceptions.Any();
    }
}