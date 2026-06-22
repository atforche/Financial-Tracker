using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

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
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, GetAccounts(request), out exceptions))
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
        out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUpdate(transaction, request, GetAccounts(transaction, request), out exceptions))
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
    public bool TryPost(
        SpendingTransaction transaction,
        AccountId accountId,
        DateOnly postedDate,
        out IEnumerable<Exception> exceptions)
    {
        if (!ValidatePosting(transaction, accountId, postedDate, out exceptions))
        {
            return false;
        }
        transaction.SetPostedDate(accountId, postedDate);
        PostTransaction(transaction, accountId);
        return true;
    }

    /// <summary>
    /// Attempts to unpost an existing Spending Transaction
    /// </summary>
    public bool TryUnpost(SpendingTransaction transaction, out IEnumerable<Exception> exceptions)
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
    public bool TryDelete(SpendingTransaction transaction, out IEnumerable<Exception> exceptions)
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
    protected bool ValidateCreate(
        CreateSpendingTransactionRequest request,
        IReadOnlyCollection<Account> accounts,
        out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(
            request,
            accounts,
            GetFunds(request),
            out exceptions);

        if (request.Source.PostedDate.HasValue || request.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new InvalidDateException("Posted dates cannot be set directly when creating a spending transaction"));
        }
        if (!ValidateAccount(request.Source, request.Destinations, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateAmounts(request.Amount, request.Destinations, out IEnumerable<Exception> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        if (!ValidateDestinationFundAssignments(request.Destinations, out IEnumerable<Exception> destinationFundAssignmentExceptions))
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
        IReadOnlyCollection<Account> accounts,
        out IEnumerable<Exception> exceptions)
    {
        _ = ValidateUpdate(transaction, request, accounts, GetFunds(request), out exceptions);

        if (transaction.Source.PostedDate.HasValue || transaction.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction has already been posted and cannot be updated"));
        }
        if (request.Source.PostedDate.HasValue || request.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Posted dates cannot be set directly when updating a spending transaction"));
        }
        if (!ValidateAccount(request.Source, request.Destinations, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateAmounts(request.Amount, request.Destinations, out IEnumerable<Exception> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        if (!ValidateDestinationFundAssignments(request.Destinations, out IEnumerable<Exception> destinationFundAssignmentExceptions))
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
        IReadOnlyCollection<FundAmount> fundAssignments,
        out IEnumerable<Exception> exceptions)
    {
        _ = base.ValidateFundAssignments(amount, fundAssignments, out exceptions);

        if (fundAssignments.Any(fundAmount => fundAmount.FundId == Fund.UnassignedFundId))
        {
            exceptions = exceptions.Append(new InvalidFundAmountException("Cannot spend money from the unassigned fund"));
        }
        if (fundAssignments.Sum(fundAmount => fundAmount.Amount) != amount)
        {
            exceptions = exceptions.Append(new InvalidFundAmountException("Total amount assigned to funds must equal the transaction amount"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Account for this Spending Transaction
    /// </summary>
    private static bool ValidateAccount(
        SpendingTransactionSource source,
        IReadOnlyCollection<SpendingTransactionDestination> destinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!source.Account.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions must debit a tracked account"));
        }
        foreach (SpendingTransactionDestination destination in destinations)
        {
            if (destination.Account == null && string.IsNullOrWhiteSpace(destination.Location))
            {
                exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions must have either a Destination Account or a Destination Location"));
            }
            if (destination.Account != null && !string.IsNullOrWhiteSpace(destination.Location))
            {
                exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions cannot have both a Destination Account and a Destination Location"));
            }
            if (destination.Account != null && destination.Account.Type.IsTracked())
            {
                exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions cannot credit a tracked account"));
            }
            if (destination.Account?.Id == source.Account?.Id)
            {
                exceptions = exceptions.Append(new InvalidAccountException("Source and destination accounts cannot be the same"));
            }
        }
        return !exceptions.Any();
    }

    private static bool ValidateAmounts(
        decimal amount,
        IReadOnlyCollection<SpendingTransactionDestination> destinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions must have at least one spending destination"));
        }
        if (destinations.Any(destination => destination.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Spending destination amounts must be positive"));
        }
        var destinationAccountIds = destinations
            .Where(destination => destination.Account != null)
            .Select(destination => destination.Account!.Id)
            .ToList();
        if (destinationAccountIds.Distinct().Count() != destinationAccountIds.Count)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Duplicate destination accounts are not allowed"));
        }
        var destinationLocations = destinations
            .Where(destination => !string.IsNullOrWhiteSpace(destination.Location))
            .Select(destination => destination.Location)
            .ToList();
        if (destinationLocations.Distinct().Count() != destinationLocations.Count)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Duplicate destination locations are not allowed"));
        }
        if (destinations.Sum(destination => destination.Amount) != amount)
        {
            exceptions = exceptions.Append(new InvalidAmountException("Spending destination amounts must equal the transaction amount"));
        }
        return !exceptions.Any();
    }

    private bool ValidateDestinationFundAssignments(IReadOnlyCollection<SpendingTransactionDestination> destinations, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        foreach (SpendingTransactionDestination destination in destinations)
        {
            if (!ValidateFundAssignments(destination.Amount, destination.FundAssignments, out IEnumerable<Exception> fundAssignmentExceptions))
            {
                exceptions = exceptions.Concat(fundAssignmentExceptions);
            }
        }
        return !exceptions.Any();
    }

    private static List<Account> GetAccounts(CreateSpendingTransactionRequest request) =>
        new List<Account?> { request.Source.Account }
            .Concat(request.Destinations.Select(destination => destination.Account))
            .OfType<Account>()
            .ToList();

    private static List<Account> GetAccounts(SpendingTransaction transaction, UpdateSpendingTransactionRequest request) =>
        new List<Account?> { transaction.Source.Account }
            .Concat(request.Destinations.Select(destination => destination.Account))
            .OfType<Account>()
            .ToList();

    private List<Fund> GetFunds(CreateSpendingTransactionRequest request) =>
        request.Destinations
            .SelectMany(destination => destination.FundAssignments)
            .Select(fundAmount => fundRepository.GetById(fundAmount.FundId))
            .ToList();

    private List<Fund> GetFunds(UpdateSpendingTransactionRequest request) =>
        request.Destinations
            .SelectMany(destination => destination.FundAssignments)
            .Select(fundAmount => fundRepository.GetById(fundAmount.FundId))
            .ToList();
}