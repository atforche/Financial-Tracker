using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Service for managing Account Transactions
/// </summary>
public class AccountTransactionService(
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
    /// Attempts to create a new Account Transaction
    /// </summary>
    public bool TryCreate(
        CreateAccountTransactionRequest request,
        [NotNullWhen(true)] out AccountTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new AccountTransaction(request, sequence);
        AddTransaction(transaction);
        if (exceptions.Any())
        {
            transaction = null;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Account Transaction
    /// </summary>
    public bool TryUpdate(
        AccountTransaction transaction,
        UpdateAccountTransactionRequest request,
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
                transaction.UpdateAccountSource(request.Source);
                transaction.UpdateAccountDestinations(request.Destinations);
            });
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Account Transaction to a specific Account
    /// </summary>
    public bool TryPost(
        AccountTransaction transaction,
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
    /// Attempts to unpost an existing Account Transaction
    /// </summary>
    public bool TryUnpost(AccountTransaction transaction, out IEnumerable<Exception> exceptions)
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
    /// Attempts to delete an existing Account Transaction
    /// </summary>
    public bool TryDelete(AccountTransaction transaction, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateDelete(transaction, out exceptions))
        {
            return false;
        }
        DeleteTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Account Transaction
    /// </summary>
    private bool ValidateCreate(CreateAccountTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        _ = ValidateCreate(request, GetAccounts(request), [], out exceptions);

        if (request.Source.PostedDate.HasValue || request.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new InvalidDateException("Posted dates cannot be set directly when creating an account transaction"));
        }
        if (!ValidateAccounts(request.Source, request.Destinations, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateAmounts(request.Amount, request.Destinations, out IEnumerable<Exception> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates a request to update an existing Account Transaction
    /// </summary>
    private bool ValidateUpdate(
        AccountTransaction transaction,
        UpdateAccountTransactionRequest request,
        out IEnumerable<Exception> exceptions)
    {
        _ = ValidateUpdate(transaction, request, GetAccounts(request), [], out exceptions);

        if (transaction.Source.PostedDate.HasValue || transaction.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction has already been posted and cannot be updated"));
        }
        if (request.Source.PostedDate.HasValue || request.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Posted dates cannot be set directly when updating an account transaction"));
        }
        if (!ValidateAccounts(request.Source, request.Destinations, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateAmounts(request.Amount, request.Destinations, out IEnumerable<Exception> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Accounts for this Account Transaction
    /// </summary>
    private static bool ValidateAccounts(
        AccountTransactionSource source,
        IReadOnlyCollection<AccountTransactionDestination> destinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (source.Account == null && string.IsNullOrWhiteSpace(source.Location))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Account Transactions must have either a Source Account or a Source Location"));
        }
        if (source.Account != null && !string.IsNullOrWhiteSpace(source.Location))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Account Transactions cannot have both a Source Account and a Source Location"));
        }
        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Account Transactions must have at least one account destination"));
        }
        foreach (AccountTransactionDestination destination in destinations)
        {
            if (destination.Account == null && string.IsNullOrWhiteSpace(destination.Location))
            {
                exceptions = exceptions.Append(new InvalidAccountException("Account Transactions must have either a Destination Account or a Destination Location"));
            }
            if (destination.Account != null && !string.IsNullOrWhiteSpace(destination.Location))
            {
                exceptions = exceptions.Append(new InvalidAccountException("Account Transactions cannot have both a Destination Account and a Destination Location"));
            }
            if (destination.Account?.Id == source.Account?.Id)
            {
                exceptions = exceptions.Append(new InvalidAccountException("Source and destination accounts cannot be the same"));
            }
            if (source.Account != null && destination.Account != null &&
                source.Account.Type.IsTracked() != destination.Account.Type.IsTracked())
            {
                exceptions = exceptions.Append(new InvalidAccountException("An Account Transaction cannot transfer between a tracked account and an untracked account"));
            }
            if (source.Account != null && source.Account.Type.IsTracked() && destination.Account == null)
            {
                exceptions = exceptions.Append(new InvalidAccountException("A one-sided Account Transaction cannot debit money from a tracked account"));
            }
            if (source.Account == null && destination.Account != null && destination.Account.Type.IsTracked())
            {
                exceptions = exceptions.Append(new InvalidAccountException("A one-sided Account Transaction cannot credit money to a tracked account"));
            }
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
        return !exceptions.Any();
    }

    private static bool ValidateAmounts(
        decimal amount,
        IReadOnlyCollection<AccountTransactionDestination> destinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException("Account Transactions must have at least one account destination"));
        }
        if (destinations.Any(destination => destination.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Account destination amounts must be positive"));
        }
        if (Math.Round(destinations.Sum(destination => destination.Amount), 2) != Math.Round(amount, 2))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Account destination amounts must equal the transaction amount"));
        }
        return !exceptions.Any();
    }

    private static List<Account> GetAccounts(CreateAccountTransactionRequest request) =>
        new List<Account?> { request.Source.Account }
            .Concat(request.Destinations.Select(destination => destination.Account))
            .OfType<Account>()
            .ToList();

    private static List<Account> GetAccounts(UpdateAccountTransactionRequest request) =>
        new List<Account?> { request.Source.Account }
            .Concat(request.Destinations.Select(destination => destination.Account))
            .OfType<Account>()
            .ToList();
}