using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Validation;

namespace Domain.Transactions.Spending;

/// <summary>
/// Service for managing Spending Transactions
/// </summary>
public class SpendingTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    IAccountingPeriodRepository accountingPeriodRepository,
    IFundRepository fundRepository,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService,
        accountingPeriodBalanceService,
        fundBalanceService,
        accountingPeriodRepository,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Spending Transaction
    /// </summary>
    public bool TryCreate(
        CreateSpendingTransactionRequest request,
        [NotNullWhen(true)] out SpendingTransaction? transaction,
        out IEnumerable<ValidationError> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new SpendingTransaction(request, sequence);
        AddTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Spending Transaction
    /// </summary>
    public bool TryUpdate(
        SpendingTransaction transaction,
        UpdateSpendingTransactionRequest request,
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
                transaction.UpdateSpendingSource(request.Source);
                transaction.UpdateSpendingDestinations(request.Destinations);
            });
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Spending Transaction to a specific Account
    /// </summary>
    public bool TryPost(SpendingTransaction transaction, PostTransactionRequest request, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidatePosting(transaction, request, out exceptions))
        {
            return false;
        }
        transaction.SetPostedDate(request.AccountId, request.PostedDate);
        PostTransaction(transaction, request.AccountId);
        return true;
    }

    /// <summary>
    /// Attempts to unpost an existing Spending Transaction
    /// </summary>
    public bool TryUnpost(SpendingTransaction transaction, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateUnposting(transaction, out exceptions))
        {
            return false;
        }
        UnpostTransaction(transaction);
        transaction.ClearPostedDates();
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Spending Transaction
    /// </summary>
    public bool TryDelete(SpendingTransaction transaction, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateDelete(transaction, out exceptions))
        {
            return false;
        }
        DeleteTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Spending Transaction
    /// </summary>
    protected bool ValidateCreate(CreateSpendingTransactionRequest request, out IEnumerable<ValidationError> exceptions)
    {
        _ = ValidateCreate(
            request,
            request.Source.Account,
            new ValidationErrorPath(nameof(CreateSpendingTransactionRequest.Source)).Append(nameof(SpendingTransactionSource.Account)),
            request.Destinations.Select(d => d.Account).ToList(),
            (i) => new ValidationErrorPath(nameof(CreateSpendingTransactionRequest.Destinations), i).Append(nameof(SpendingTransactionDestination.Account)),
            [],
            (i) => ValidationErrorPath.Empty,
            request.Destinations.Select(destination => destination.FundAssignments.Select(fundAssignment => fundRepository.GetById(fundAssignment.FundId)).ToList()).ToList(),
            (i, j) => new ValidationErrorPath(nameof(CreateSpendingTransactionRequest.Destinations), i)
                .AppendWithIndex(nameof(SpendingTransactionDestination.FundAssignments), j),
            out exceptions);

        if (request.Source.PostedDate.HasValue)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateSpendingTransactionRequest.Source))
                    .Append(nameof(SpendingTransactionSource.PostedDate)),
                "Posted dates cannot be set directly when creating a spending transaction"));
        }
        foreach ((int index, SpendingTransactionDestination destination) in request.Destinations.Index())
        {
            if (destination.PostedDate.HasValue)
            {
                exceptions = exceptions.Append(new ValidationError(
                    new ValidationErrorPath(nameof(CreateSpendingTransactionRequest.Destinations), index)
                        .Append(nameof(SpendingTransactionDestination.PostedDate)),
                    "Posted dates cannot be set directly when creating a spending transaction"));
            }
        }
        if (!ValidateAccount(
                request.Source,
                new ValidationErrorPath(nameof(CreateSpendingTransactionRequest.Source)),
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(CreateSpendingTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateAmounts(
                request.Amount,
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(CreateSpendingTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        if (!ValidateDestinationFundAssignments(
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(CreateSpendingTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> destinationFundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(destinationFundAssignmentExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to update an existing Spending Transaction
    /// </summary>
    protected bool ValidateUpdate(
        SpendingTransaction transaction,
        UpdateSpendingTransactionRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        _ = ValidateUpdate(transaction,
            request,
            request.Source.Account,
            new ValidationErrorPath(nameof(UpdateSpendingTransactionRequest.Source)).Append(nameof(SpendingTransactionSource.Account)),
            request.Destinations.Select(d => d.Account).ToList(),
            (i) => new ValidationErrorPath(nameof(UpdateSpendingTransactionRequest.Destinations), i).Append(nameof(SpendingTransactionDestination.Account)),
            [],
            (i) => ValidationErrorPath.Empty,
            request.Destinations.Select(destination => destination.FundAssignments.Select(fundAssignment => fundRepository.GetById(fundAssignment.FundId)).ToList()).ToList(),
            (i, j) => new ValidationErrorPath(nameof(UpdateSpendingTransactionRequest.Destinations), i)
                .AppendWithIndex(nameof(SpendingTransactionDestination.FundAssignments), j),
            out exceptions);

        if (transaction.Source.PostedDate.HasValue || transaction.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Transaction has already been posted and cannot be updated"));
        }
        if (request.Source.PostedDate.HasValue)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(SpendingTransactionSource.PostedDate))
                    .Prepend(nameof(UpdateSpendingTransactionRequest.Source)),
                "Posted dates cannot be set directly when updating a spending transaction"));
        }
        foreach ((int index, SpendingTransactionDestination destination) in request.Destinations.Index())
        {
            if (destination.PostedDate.HasValue)
            {
                exceptions = exceptions.Append(new ValidationError(
                    new ValidationErrorPath(nameof(SpendingTransactionDestination.PostedDate))
                        .PrependWithIndex(nameof(UpdateSpendingTransactionRequest.Destinations), index),
                    "Posted dates cannot be set directly when updating a spending transaction"));
            }
        }
        if (!ValidateAccount(
                request.Source,
                new ValidationErrorPath(nameof(UpdateSpendingTransactionRequest.Source)),
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(UpdateSpendingTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateAmounts(
                request.Amount,
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(UpdateSpendingTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        if (!ValidateDestinationFundAssignments(
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(UpdateSpendingTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> destinationFundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(destinationFundAssignmentExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Fund Assignments for this Spending Transaction
    /// </summary>
    protected override bool ValidateFundAssignments(
        decimal amount,
        ValidationErrorPath amountPath,
        IReadOnlyCollection<FundAmount> fundAssignments,
        Func<int, ValidationErrorPath> fundAssignmentsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        _ = base.ValidateFundAssignments(amount, amountPath, fundAssignments, fundAssignmentsPathBuilder, out exceptions);

        foreach ((int index, FundAmount fundAmount) in fundAssignments.Index())
        {
            if (fundAmount.FundId == Fund.UnassignedFundId)
            {
                exceptions = exceptions.Append(new ValidationError(
                    fundAssignmentsPathBuilder(index).Append(nameof(FundAmount.FundId)),
                    "Cannot spend money from the unassigned fund"));
            }
        }
        if (fundAssignments.Sum(fundAmount => fundAmount.Amount) != amount)
        {
            exceptions = exceptions.Append(new ValidationError(
                amountPath,
                "Total amount assigned to funds must equal the transaction amount"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Account for this Spending Transaction
    /// </summary>
    private static bool ValidateAccount(
        SpendingTransactionSource source,
        ValidationErrorPath sourcePath,
        IReadOnlyCollection<SpendingTransactionDestination> destinations,
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!source.Account.Type.IsTracked())
        {
            exceptions = exceptions.Append(new ValidationError(sourcePath.Append(nameof(SpendingTransactionSource.Account)), "Spending Transactions must debit a tracked account"));
        }
        foreach ((int index, SpendingTransactionDestination destination) in destinations.Index())
        {
            if (destination.Account == null && string.IsNullOrWhiteSpace(destination.Location))
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Account)),
                    "Spending Transactions must have either a Destination Account or a Destination Location"));
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Location)),
                    "Spending Transactions must have either a Destination Account or a Destination Location"));
            }
            if (destination.Account != null && !string.IsNullOrWhiteSpace(destination.Location))
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Account)),
                    "Spending Transactions cannot have both a Destination Account and a Destination Location"));
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Location)),
                    "Spending Transactions cannot have both a Destination Account and a Destination Location"));
            }
            if (destination.Account != null && destination.Account.Type.IsTracked())
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Account)),
                    "Spending Transactions cannot credit a tracked account"));
            }
            if (destination.Account?.Id == source.Account?.Id)
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Account)),
                    "Source and destination accounts cannot be the same"));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the amounts for this Spending Transaction
    /// </summary>
    private static bool ValidateAmounts(
        decimal amount,
        IReadOnlyCollection<SpendingTransactionDestination> destinations,
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(0), "Spending Transactions must have at least one spending destination"));
        }
        foreach ((int index, SpendingTransactionDestination destination) in destinations.Index())
        {
            if (destination.Amount <= 0)
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Amount)),
                    "Spending destination amounts must be positive"));
            }
            if (destination.Account != null && destinations.Index().Any(pair => pair.Item.Account == destination.Account && pair.Index != index))
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Account)),
                    "Duplicate destination accounts are not allowed"));
            }
            if (!string.IsNullOrWhiteSpace(destination.Location) && destinations.Index().Any(pair => pair.Item.Location == destination.Location && pair.Index != index))
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Location)),
                    "Duplicate destination locations are not allowed"));
            }
            if (Math.Round(destinations.Sum(destination => destination.Amount), 2) != amount)
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Amount)),
                    "Spending destination amounts must equal the transaction amount"));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the structure of this Spending Transaction, including its source and destinations
    /// </summary>
    private bool ValidateDestinationFundAssignments(
        IReadOnlyCollection<SpendingTransactionDestination> destinations,
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        foreach ((int index, SpendingTransactionDestination destination) in destinations.Index())
        {
            if (!ValidateFundAssignments(
                    destination.Amount,
                    destinationsPathBuilder(index).Append(nameof(SpendingTransactionDestination.Amount)),
                    destination.FundAssignments,
                    (i) => destinationsPathBuilder(index).AppendWithIndex(nameof(SpendingTransactionDestination.FundAssignments), i),
                    out IEnumerable<ValidationError> fundAssignmentExceptions))
            {
                exceptions = exceptions.Concat(fundAssignmentExceptions);
            }
        }
        return !exceptions.Any();
    }
}