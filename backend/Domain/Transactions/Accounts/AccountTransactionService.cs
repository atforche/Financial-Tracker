using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Validation;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Service for managing Account Transactions
/// </summary>
public class AccountTransactionService(
    AccountBalanceService accountBalanceService,
    PendingAccountBalanceService pendingAccountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    PendingFundBalanceService pendingFundBalanceService,
    FundGoalTotalsHistoryService fundGoalTotalsHistoryService,
    PendingFundGoalTotalsService pendingFundGoalTotalsService,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService,
        pendingAccountBalanceService,
        accountingPeriodBalanceService,
        fundBalanceService,
        pendingFundBalanceService,
        fundGoalTotalsHistoryService,
        pendingFundGoalTotalsService,
        accountingPeriodRepository,
        transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Account Transaction
    /// </summary>
    public bool TryCreate(
        CreateAccountTransactionRequest request,
        [NotNullWhen(true)] out AccountTransaction? transaction,
        out IEnumerable<ValidationError> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new AccountTransaction(request, sequence);
        AddTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Account Transaction
    /// </summary>
    public bool TryUpdate(
        AccountTransaction transaction,
        UpdateAccountTransactionRequest request,
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
    /// Attempts to unpost an existing Account Transaction
    /// </summary>
    public bool TryUnpost(AccountTransaction transaction, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateUnposting(transaction, out exceptions))
        {
            return false;
        }
        UnpostTransaction(transaction);
        transaction.ClearPostedDates();
        SynchronizePendingAccountBalanceEffects(transaction);
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Account Transaction
    /// </summary>
    public bool TryDelete(AccountTransaction transaction, out IEnumerable<ValidationError> exceptions)
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
    private bool ValidateCreate(CreateAccountTransactionRequest request, out IEnumerable<ValidationError> exceptions)
    {
        _ = ValidateCreate(
            request,
            request.Source.Account,
            new ValidationErrorPath(nameof(CreateAccountTransactionRequest.Source)).Append(nameof(AccountTransactionSource.Account)),
            request.Destinations.Select(destination => destination.Account).ToList(),
            (index) => new ValidationErrorPath(nameof(CreateAccountTransactionRequest.Destinations), index).Append(nameof(AccountTransactionDestination.Account)),
            [],
            (i) => ValidationErrorPath.Empty,
            [],
            (i, j) => ValidationErrorPath.Empty,
            out exceptions);

        if (request.Source.PostedDate.HasValue || request.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new ValidationError(new ValidationErrorPath(nameof(CreateAccountTransactionRequest.Source)), "Posted dates cannot be set directly when creating an account transaction"));
        }
        if (!ValidateAccounts(
                request.Source,
                new ValidationErrorPath(nameof(CreateAccountTransactionRequest.Source)),
                request.Destinations,
                index => new ValidationErrorPath(nameof(CreateAccountTransactionRequest.Destinations), index),
                out IEnumerable<ValidationError> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateAmounts(
                request.Amount,
                request.Destinations,
                index => new ValidationErrorPath(nameof(CreateAccountTransactionRequest.Destinations), index),
                out IEnumerable<ValidationError> amountExceptions))
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
        out IEnumerable<ValidationError> exceptions)
    {
        _ = ValidateUpdate(
            transaction,
            request,
            request.Source.Account,
            new ValidationErrorPath(nameof(UpdateAccountTransactionRequest.Source)).Append(nameof(AccountTransactionSource.Account)),
            request.Destinations.Select(destination => destination.Account).ToList(),
            (index) => new ValidationErrorPath(nameof(UpdateAccountTransactionRequest.Destinations), index).Append(nameof(AccountTransactionDestination.Account)),
            [],
            (i) => ValidationErrorPath.Empty,
            [],
            (i, j) => ValidationErrorPath.Empty,
            out exceptions);

        if (transaction.Source.PostedDate.HasValue || transaction.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Transaction has already been posted and cannot be updated"));
        }
        if (request.Source.PostedDate.HasValue || request.Destinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Posted dates cannot be set directly when updating an account transaction"));
        }
        if (!ValidateAccounts(
                request.Source,
                new ValidationErrorPath(nameof(UpdateAccountTransactionRequest.Source)),
                request.Destinations,
                index => new ValidationErrorPath(nameof(UpdateAccountTransactionRequest.Destinations), index),
                out IEnumerable<ValidationError> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateAmounts(
                request.Amount,
                request.Destinations,
                index => new ValidationErrorPath(nameof(UpdateAccountTransactionRequest.Destinations), index),
                out IEnumerable<ValidationError> amountExceptions))
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
        ValidationErrorPath sourcePath,
        IReadOnlyCollection<AccountTransactionDestination> destinations,
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (source.Account == null && source.Location == null)
        {
            exceptions = exceptions.Append(new ValidationError(
                sourcePath.Append(nameof(AccountTransactionSource.Account)),
                "Account Transactions must have either a Source Account or a Source Location"));
            exceptions = exceptions.Append(new ValidationError(
                sourcePath.Append(nameof(AccountTransactionSource.Location)),
                "Account Transactions must have either a Source Account or a Source Location"));
        }
        if (source.Account != null && source.Location != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                sourcePath.Append(nameof(AccountTransactionSource.Account)),
                "Account Transactions cannot have both a Source Account and a Source Location"));
            exceptions = exceptions.Append(new ValidationError(
                sourcePath.Append(nameof(AccountTransactionSource.Location)),
                "Account Transactions cannot have both a Source Account and a Source Location"));
        }
        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new ValidationError(
                destinationsPathBuilder(0),
                "Account Transactions must have at least one account destination"));
        }
        foreach ((int index, AccountTransactionDestination destination) in destinations.Index())
        {
            if (destination.Account == null && destination.Location == null)
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Account)),
                    "Account Transactions must have either a Destination Account or a Destination Location"));
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Location)),
                    "Account Transactions must have either a Destination Account or a Destination Location"));
            }
            if (destination.Account != null && destination.Location != null)
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Account)),
                    "Account Transactions cannot have both a Destination Account and a Destination Location"));
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Location)),
                    "Account Transactions cannot have both a Destination Account and a Destination Location"));
            }
            if (destination.Account?.Id == source.Account?.Id)
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Account)),
                    "Source and destination accounts cannot be the same"));
            }
            if (source.Account != null && destination.Account != null &&
                source.Account.Type.IsTracked() != destination.Account.Type.IsTracked())
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Account)),
                    "An Account Transaction cannot transfer between a tracked account and an untracked account"));
            }
            if (source.Account != null && source.Account.Type.IsTracked() && destination.Account == null)
            {
                exceptions = exceptions.Append(new ValidationError(
                    sourcePath.Append(nameof(AccountTransactionSource.Account)),
                    "A one-sided Account Transaction cannot debit money from a tracked account"));
            }
            if (source.Account == null && destination.Account != null && destination.Account.Type.IsTracked())
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Account)),
                    "A one-sided Account Transaction cannot credit money to a tracked account"));
            }
            if (destination.Account != null && destinations.Index().Any(pair =>
                pair.Index != index && pair.Item.Account?.Id == destination.Account.Id))
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Account)),
                    "Duplicate destination accounts are not allowed"));
            }
            if (destination.Location != null && destinations.Index().Any(pair =>
                pair.Index != index && pair.Item.Location == destination.Location))
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Location)),
                    "Duplicate destination locations are not allowed"));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the accounts for this Account Transaction
    /// </summary>
    private static bool ValidateAmounts(
        decimal amount,
        IReadOnlyCollection<AccountTransactionDestination> destinations,
        Func<int, ValidationErrorPath> destinationsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (destinations.Count == 0)
        {
            exceptions = exceptions.Append(new ValidationError(
                destinationsPathBuilder(0),
                "Account Transactions must have at least one account destination"));
        }
        foreach ((int index, AccountTransactionDestination destination) in destinations.Index())
        {
            if (destination.Amount <= 0)
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Amount)),
                    "Account destination amounts must be positive"));
            }
            if (Math.Round(destinations.Sum(destination => destination.Amount), 2) != Math.Round(amount, 2))
            {
                exceptions = exceptions.Append(new ValidationError(
                    destinationsPathBuilder(index).Append(nameof(AccountTransactionDestination.Amount)),
                    "Account destination amounts must equal the transaction amount"));
            }
        }
        return !exceptions.Any();
    }
}