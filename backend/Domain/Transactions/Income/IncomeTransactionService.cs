using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Validation;

namespace Domain.Transactions.Income;

/// <summary>
/// Service for managing Income Transactions
/// </summary>
public class IncomeTransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    FundPlanTotalsHistoryService fundPlanTotalsHistoryService,
    IAccountingPeriodRepository accountingPeriodRepository,
    IFundRepository fundRepository,
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
    /// Attempts to create a new Income Transaction
    /// </summary>
    public bool TryCreate(
        CreateIncomeTransactionRequest request,
        [NotNullWhen(true)] out IncomeTransaction? transaction,
        out IEnumerable<ValidationError> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new IncomeTransaction(request, sequence);
        AddTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Income Transaction
    /// </summary>
    public bool TryUpdate(
        IncomeTransaction transaction,
        UpdateIncomeTransactionRequest request,
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
                transaction.UpdateIncomeSource(request.Source);
                transaction.UpdateIncomeDestinations(request.Destinations);
            });
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Income Transaction to a specific Account
    /// </summary>
    public bool TryPost(
        IncomeTransaction transaction,
        PostTransactionRequest request,
        out IEnumerable<ValidationError> exceptions)
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
    /// Attempts to unpost an existing Income Transaction
    /// </summary>
    public bool TryUnpost(IncomeTransaction transaction, out IEnumerable<ValidationError> exceptions)
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
    /// Attempts to delete an existing Income Transaction
    /// </summary>
    public bool TryDelete(IncomeTransaction transaction, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateDelete(transaction, out exceptions))
        {
            return false;
        }
        DeleteTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Income Transaction
    /// </summary>
    protected bool ValidateCreate(CreateIncomeTransactionRequest request, out IEnumerable<ValidationError> exceptions)
    {
        _ = ValidateCreate(
            request,
            request.Source.Account,
            new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Source)).Append(nameof(CreateIncomeTransactionRequest.Source.Account)),
            request.Destinations.Select(destination => destination.Account).ToList(),
            (i) => new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Destinations), i),
            [],
            (i) => ValidationErrorPath.Empty,
            request.Destinations.Select(destination => destination.FundAssignments.Select(fundAssignment => fundRepository.GetById(fundAssignment.FundId)).ToList()).ToList(),
            (i, j) => new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Destinations), i).AppendWithIndex(nameof(IncomeTransactionDestination.FundAssignments), j),
            out exceptions);

        if (request.Source.PostedDate.HasValue)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Source))
                    .Append(nameof(IncomeTransactionSource.PostedDate)),
                "Posted dates cannot be set directly when creating an income transaction"));
        }
        foreach ((int index, IncomeTransactionDestination destination) in request.Destinations.Index())
        {
            if (destination.PostedDate.HasValue)
            {
                exceptions = exceptions.Append(new ValidationError(
                    new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Destinations), index)
                        .Append(nameof(IncomeTransactionDestination.PostedDate)),
                    "Posted dates cannot be set directly when creating an income transaction"));
            }
        }
        if (!ValidateAccount(
                request.Source,
                new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Source)),
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateIncomeStructure(
                request.Amount,
                new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Amount)),
                request.Source,
                new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Source)),
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> structureExceptions))
        {
            exceptions = exceptions.Concat(structureExceptions);
        }
        if (!ValidateDestinationFundAssignments(
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(CreateIncomeTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> fundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(fundAssignmentExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to update an existing Income Transaction
    /// </summary>
    protected bool ValidateUpdate(
        IncomeTransaction transaction,
        UpdateIncomeTransactionRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        _ = ValidateUpdate(transaction, request,
            request.Source.Account,
            new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Source)).Append(nameof(UpdateIncomeTransactionRequest.Source.Account)),
            request.Destinations.Select(destination => destination.Account).ToList(),
            (i) => new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Destinations), i),
            [],
            (i) => ValidationErrorPath.Empty,
            request.Destinations.Select(destination => destination.FundAssignments.Select(fundAssignment => fundRepository.GetById(fundAssignment.FundId)).ToList()).ToList(),
            (i, j) => new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Destinations), i).AppendWithIndex(nameof(IncomeTransactionDestination.FundAssignments), j),
            out exceptions);

        if (transaction.Source.PostedDate.HasValue || transaction.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Transaction has already been posted and cannot be updated"));
        }
        if (request.Source.PostedDate.HasValue)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Source))
                    .Append(nameof(IncomeTransactionSource.PostedDate)),
                "Posted dates cannot be set directly when updating an income transaction"));
        }
        foreach ((int index, IncomeTransactionDestination destination) in request.Destinations.Index())
        {
            if (destination.PostedDate.HasValue)
            {
                exceptions = exceptions.Append(new ValidationError(
                    new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Destinations), index)
                        .Append(nameof(IncomeTransactionDestination.PostedDate)),
                    "Posted dates cannot be set directly when updating an income transaction"));
            }
        }
        if (!ValidateAccount(
                request.Source,
                new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Source)),
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateIncomeStructure(
                request.Amount,
                new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Amount)),
                request.Source,
                new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Source)),
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> structureExceptions))
        {
            exceptions = exceptions.Concat(structureExceptions);
        }
        if (!ValidateDestinationFundAssignments(
                request.Destinations,
                (i) => new ValidationErrorPath(nameof(UpdateIncomeTransactionRequest.Destinations), i),
                out IEnumerable<ValidationError> fundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(fundAssignmentExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the accounts for this Income Transaction
    /// </summary>
    private static bool ValidateAccount(
        IncomeTransactionSource source,
        ValidationErrorPath sourcePath,
        IReadOnlyCollection<IncomeTransactionDestination> destinations,
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (source.Account != null && source.Account.Type.IsTracked())
        {
            exceptions = exceptions.Append(new ValidationError(sourcePath.Append(nameof(IncomeTransactionSource.Account)), "Income Transactions cannot source money from a tracked account"));
        }
        if (source.Account == null && string.IsNullOrWhiteSpace(source.Location))
        {
            exceptions = exceptions.Append(new ValidationError(sourcePath.Append(nameof(IncomeTransactionSource.Account)), "Income Transactions must have either a Source Account or a Source Location"));
            exceptions = exceptions.Append(new ValidationError(sourcePath.Append(nameof(IncomeTransactionSource.Location)), "Income Transactions must have either a Source Account or a Source Location"));
        }
        if (source.Account != null && !string.IsNullOrWhiteSpace(source.Location))
        {
            exceptions = exceptions.Append(new ValidationError(sourcePath.Append(nameof(IncomeTransactionSource.Account)), "Income Transactions cannot have both a Source Account and a Source Location"));
            exceptions = exceptions.Append(new ValidationError(sourcePath.Append(nameof(IncomeTransactionSource.Location)), "Income Transactions cannot have both a Source Account and a Source Location"));
        }
        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(0), "Income Transactions must have at least one income destination"));
        }
        foreach ((int index, IncomeTransactionDestination destination) in destinations.Index())
        {
            if (!destination.Account.Type.IsTracked())
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(IncomeTransactionDestination.Account)),
                    "Income Transactions must deposit into a tracked account"));
            }
            if (source.Account != null && destination.Account?.Id == source.Account.Id)
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(IncomeTransactionDestination.Account)),
                    "Source and destination accounts cannot be the same"));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the structure of this Income Transaction, including its income lines, deductions, and destination fund assignments
    /// </summary>
    private static bool ValidateIncomeStructure(
        decimal amount,
        ValidationErrorPath amountPath,
        IncomeTransactionSource source,
        ValidationErrorPath sourcePath,
        IReadOnlyCollection<IncomeTransactionDestination> destinations,
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (source.IncomeLines.Count == 0)
        {
            exceptions = exceptions.Append(new ValidationError(sourcePath, "Income Transactions must have at least one income line"));
        }
        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(0), "Income Transactions must have at least one income destination"));
        }
        foreach ((int index, IncomeLine incomeLine) in source.IncomeLines.Index())
        {
            if (incomeLine.Amount <= 0)
            {
                exceptions = exceptions.Append(new ValidationError(sourcePath.AppendWithIndex(nameof(IncomeTransactionSource.IncomeLines), index), "Income line amounts must be positive"));
            }
        }
        foreach ((int index, IncomeDeduction deduction) in source.IncomeDeductions.Index())
        {
            if (deduction.Amount <= 0)
            {
                exceptions = exceptions.Append(new ValidationError(sourcePath.AppendWithIndex(nameof(IncomeTransactionSource.IncomeDeductions), index), "Income deduction amounts must be positive"));
            }
        }
        foreach ((int index, IncomeTransactionDestination destination) in destinations.Index())
        {
            if (destination.Amount <= 0)
            {
                exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(index), "Income destination amounts must be positive"));
            }
            if (destination.Account.Type.IsTracked() && destination.Amount != destination.FundAssignments.Sum(fundAmount => fundAmount.Amount))
            {
                exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(index).Append(nameof(IncomeTransactionDestination.Amount)), "Income destination amounts must equal the sum of their fund assignments"));
            }
            if (destinations.Index().Any(pair => pair.Item.Account == destination.Account && pair.Index != index))
            {
                exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(index), "Duplicate income destination accounts are not allowed"));
            }
        }
        decimal calculatedNetAmount = source.IncomeLines.Sum(line => line.Amount) - source.IncomeDeductions.Sum(deduction => deduction.Amount);
        if (Math.Round(calculatedNetAmount, 2) != Math.Round(amount, 2))
        {
            exceptions = exceptions.Append(new ValidationError(amountPath, "Income lines minus deductions must equal the transaction amount"));
        }
        decimal totalDestinationAmount = destinations.Sum(destination => destination.Amount);
        if (Math.Round(totalDestinationAmount, 2) != Math.Round(amount, 2))
        {
            exceptions = exceptions.Append(new ValidationError(amountPath, "Income destination amounts must equal the transaction amount"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the accounts for this Income Transaction
    /// </summary>
    private bool ValidateDestinationFundAssignments(
        IReadOnlyCollection<IncomeTransactionDestination> destinations,
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        foreach ((int index, IncomeTransactionDestination destination) in destinations.Index())
        {
            if (!destination.Account.Type.IsTracked() && destination.FundAssignments.Count > 0)
            {
                exceptions = exceptions.Append(new ValidationError(destinationsPathBuilder(index).Append(nameof(IncomeTransactionDestination.FundAssignments)), "Income destination fund assignments can only be specified for tracked accounts"));
            }
            if (!ValidateFundAssignments(
                    destination.Amount,
                    destinationsPathBuilder(index).Append(nameof(IncomeTransactionDestination.Amount)),
                    destination.FundAssignments,
                    (i) => destinationsPathBuilder(index).AppendWithIndex(nameof(IncomeTransactionDestination.FundAssignments), i),
                    out IEnumerable<ValidationError> fundAssignmentExceptions))
            {
                exceptions = exceptions.Concat(fundAssignmentExceptions);
            }
        }
        return !exceptions.Any();
    }
}