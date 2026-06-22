using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Transactions.Income;

/// <summary>
/// Service for managing Income Transactions
/// </summary>
public class IncomeTransactionService(
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
    /// Attempts to create a new Income Transaction
    /// </summary>
    public bool TryCreate(
        CreateIncomeTransactionRequest request,
        [NotNullWhen(true)] out IncomeTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, GetAccounts(request), out exceptions))
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
    /// Attempts to unpost an existing Income Transaction
    /// </summary>
    public bool TryUnpost(IncomeTransaction transaction, out IEnumerable<Exception> exceptions)
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
    public bool TryDelete(IncomeTransaction transaction, out IEnumerable<Exception> exceptions)
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
    protected bool ValidateCreate(
        CreateIncomeTransactionRequest request,
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
            exceptions = exceptions.Append(new InvalidDateException("Posted dates cannot be set directly when creating an income transaction"));
        }
        if (!ValidateAccount(request.Source, request.Destinations, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateIncomeStructure(request.Amount, request.Source, request.Destinations, out IEnumerable<Exception> structureExceptions))
        {
            exceptions = exceptions.Concat(structureExceptions);
        }
        if (!ValidateDestinationFundAssignments(request.Destinations, out IEnumerable<Exception> fundAssignmentExceptions))
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
            exceptions = exceptions.Append(new UnableToUpdateException("Posted dates cannot be set directly when updating an income transaction"));
        }
        if (!ValidateAccount(request.Source, request.Destinations, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateIncomeStructure(request.Amount, request.Source, request.Destinations, out IEnumerable<Exception> structureExceptions))
        {
            exceptions = exceptions.Concat(structureExceptions);
        }
        if (!ValidateDestinationFundAssignments(request.Destinations, out IEnumerable<Exception> fundAssignmentExceptions))
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
        IReadOnlyCollection<IncomeTransactionDestination> destinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (source.Account != null && source.Account.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions cannot source money from a tracked account"));
        }
        if (source.Account == null && string.IsNullOrWhiteSpace(source.Location))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions must have either a Source Account or a Source Location"));
        }
        if (source.Account != null && !string.IsNullOrWhiteSpace(source.Location))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions cannot have both a Source Account and a Source Location"));
        }
        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions must have at least one income destination"));
        }
        if (destinations.All(destination => !destination.Account.Type.IsTracked()))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions must deposit into at least one tracked account"));
        }
        if (source.Account != null && destinations.Any(destination => destination.Account.Id == source.Account.Id))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Source and destination accounts cannot be the same"));
        }
        return !exceptions.Any();
    }

    private static bool ValidateIncomeStructure(
        decimal amount,
        IncomeTransactionSource source,
        IReadOnlyCollection<IncomeTransactionDestination> destinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (source.IncomeLines.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income Transactions must have at least one income line"));
        }
        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income Transactions must have at least one income destination"));
        }
        if (source.IncomeLines.Any(line => line.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income line amounts must be positive"));
        }
        if (source.IncomeDeductions.Any(deduction => deduction.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income deduction amounts must be positive"));
        }
        if (destinations.Any(destination => destination.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income destination amounts must be positive"));
        }
        if (destinations.Any(destination => destination.Account.Type.IsTracked() && destination.Amount != destination.FundAssignments.Sum(fundAmount => fundAmount.Amount)))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income destination amounts must equal the sum of their fund assignments"));
        }
        if (destinations.Select(destination => destination.Account.Id).Distinct().Count() != destinations.Count)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Duplicate destination accounts are not allowed"));
        }
        decimal calculatedNetAmount = source.IncomeLines.Sum(line => line.Amount) - source.IncomeDeductions.Sum(deduction => deduction.Amount);
        if (Math.Round(calculatedNetAmount, 2) != Math.Round(amount, 2))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income lines minus deductions must equal the transaction amount"));
        }
        decimal totalDestinationAmount = destinations.Sum(destination => destination.Amount);
        if (Math.Round(totalDestinationAmount, 2) != Math.Round(amount, 2))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income destination amounts must equal the transaction amount"));
        }
        return !exceptions.Any();
    }

    private bool ValidateDestinationFundAssignments(IReadOnlyCollection<IncomeTransactionDestination> destinations, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        foreach (IncomeTransactionDestination destination in destinations)
        {
            if (!destination.Account.Type.IsTracked() && destination.FundAssignments.Count > 0)
            {
                exceptions = exceptions.Append(new InvalidFundAmountException("Income destination fund assignments can only be specified for tracked accounts"));
            }
            if (!ValidateFundAssignments(destination.Amount, destination.FundAssignments, out IEnumerable<Exception> fundAssignmentExceptions))
            {
                exceptions = exceptions.Concat(fundAssignmentExceptions);
            }
        }
        return !exceptions.Any();
    }

    private static List<Account> GetAccounts(CreateIncomeTransactionRequest request) =>
        new List<Account?> { request.Source.Account }
            .Concat(request.Destinations.Select(destination => destination.Account))
            .OfType<Account>()
            .ToList();

    private static List<Account> GetAccounts(IncomeTransaction transaction, UpdateIncomeTransactionRequest request) =>
        new List<Account?> { transaction.Source.Account }
            .Concat(request.Destinations.Select(destination => destination.Account))
            .OfType<Account>()
            .ToList();

    private List<Fund> GetFunds(CreateIncomeTransactionRequest request) =>
        request.Destinations
            .SelectMany(destination => destination.FundAssignments)
            .Select(fundAmount => fundRepository.GetById(fundAmount.FundId))
            .ToList();

    private List<Fund> GetFunds(UpdateIncomeTransactionRequest request) =>
        request.Destinations
            .SelectMany(destination => destination.FundAssignments)
            .Select(fundAmount => fundRepository.GetById(fundAmount.FundId))
            .ToList();
}