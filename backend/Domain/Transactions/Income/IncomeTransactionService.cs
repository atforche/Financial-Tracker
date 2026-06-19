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
    IAccountRepository accountRepository,
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

        if (!ValidateCreate(
                request,
                GetAccounts(request),
                out exceptions))
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
        if (!ValidateUpdate(
                transaction,
                request,
                GetAccounts(transaction, request),
                out exceptions))
        {
            return false;
        }
        UpdateTransaction(
            transaction,
            request,
            () =>
            {
                transaction.UpdateIncomeLines(request.IncomeLines);
                transaction.UpdateIncomeDeductions(request.IncomeDeductions);
                transaction.UpdateIncomeDestinations(request.IncomeDestinations);
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

        if (!ValidateAccount(request.SourceAccount, request.SourceLocation, request.IncomeDestinations, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateIncomeStructure(request.Amount, request.IncomeLines, request.IncomeDeductions, request.IncomeDestinations, out IEnumerable<Exception> structureExceptions))
        {
            exceptions = exceptions.Concat(structureExceptions);
        }
        if (!ValidateDestinationFundAssignments(request.IncomeDestinations, out IEnumerable<Exception> fundAssignmentExceptions))
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
        _ = base.ValidateUpdate(transaction, request, accounts, out exceptions);

        Account? sourceAccount = transaction.SourceAccountId != null ? accountRepository.GetById(transaction.SourceAccountId) : null;
        if (transaction.SourcePostedDate.HasValue || transaction.IncomeDestinations.Any(destination => destination.PostedDate.HasValue))
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction has already been posted and cannot be updated"));
        }
        if (!ValidateAccount(sourceAccount, transaction.SourceLocation, request.IncomeDestinations, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateIncomeStructure(request.Amount, request.IncomeLines, request.IncomeDeductions, request.IncomeDestinations, out IEnumerable<Exception> structureExceptions))
        {
            exceptions = exceptions.Concat(structureExceptions);
        }
        if (!ValidateDestinationFundAssignments(request.IncomeDestinations, out IEnumerable<Exception> fundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(fundAssignmentExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the accounts for this Income Transaction
    /// </summary>
    private static bool ValidateAccount(
        Account? sourceAccount,
        string? sourceLocation,
        IReadOnlyCollection<IncomeDestination> incomeDestinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (sourceAccount != null && sourceAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions cannot source money from a tracked account"));
        }
        if (sourceAccount == null && string.IsNullOrWhiteSpace(sourceLocation))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions must have either a Source Account or a Source Location"));
        }
        if (sourceAccount != null && !string.IsNullOrWhiteSpace(sourceLocation))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions cannot have both a Source Account and a Source Location"));
        }
        if (incomeDestinations.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions must have at least one income destination"));
        }
        if (incomeDestinations.All(destination => !destination.Account.Type.IsTracked()))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Income Transactions must deposit into at least one tracked account"));
        }
        if (sourceAccount != null && incomeDestinations.Any(destination => destination.Account.Id == sourceAccount.Id))
        {
            exceptions = exceptions.Append(new InvalidAccountException("Source and destination accounts cannot be the same"));
        }
        return !exceptions.Any();
    }

    private static List<Account> GetAccounts(CreateIncomeTransactionRequest request) =>
        new List<Account?> { request.SourceAccount }
            .Concat(request.IncomeDestinations.Select(destination => destination.Account))
            .OfType<Account>()
            .ToList();

    private List<Account> GetAccounts(IncomeTransaction transaction, UpdateIncomeTransactionRequest request) =>
        new List<Account?> { transaction.SourceAccountId != null ? accountRepository.GetById(transaction.SourceAccountId) : null }
            .Concat(request.IncomeDestinations.Select(destination => destination.Account))
            .OfType<Account>()
            .ToList();

    private List<Fund> GetFunds(CreateIncomeTransactionRequest request) =>
        request.IncomeDestinations
            .SelectMany(destination => destination.FundAssignments)
            .Select(fundAmount => fundRepository.GetById(fundAmount.FundId))
            .ToList();

    private static bool ValidateIncomeStructure(
        decimal amount,
        IReadOnlyCollection<IncomeLine> incomeLines,
        IReadOnlyCollection<IncomeDeduction> incomeDeductions,
        IReadOnlyCollection<IncomeDestination> incomeDestinations,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (incomeLines.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income Transactions must have at least one income line"));
        }
        if (incomeDestinations.Count == 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income Transactions must have at least one income destination"));
        }
        if (incomeLines.Any(line => line.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income line amounts must be positive"));
        }
        if (incomeDeductions.Any(deduction => deduction.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income deduction amounts must be positive"));
        }
        if (incomeDestinations.Any(destination => destination.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income destination amounts must be positive"));
        }
        if (incomeDestinations.Select(destination => destination.Account.Id).Distinct().Count() != incomeDestinations.Count)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Duplicate destination accounts are not allowed"));
        }
        decimal calculatedNetAmount = incomeLines.Sum(line => line.Amount) - incomeDeductions.Sum(deduction => deduction.Amount);
        if (Math.Round(calculatedNetAmount, 2) != Math.Round(amount, 2))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income lines minus deductions must equal the transaction amount"));
        }
        decimal totalDestinationAmount = incomeDestinations.Sum(destination => destination.Amount);
        if (Math.Round(totalDestinationAmount, 2) != Math.Round(amount, 2))
        {
            exceptions = exceptions.Append(new InvalidAmountException("Income destination amounts must equal the transaction amount"));
        }
        return !exceptions.Any();
    }

    private bool ValidateDestinationFundAssignments(IReadOnlyCollection<IncomeDestination> incomeDestinations, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        foreach (IncomeDestination destination in incomeDestinations)
        {
            if (!ValidateFundAssignments(destination.Amount, destination.FundAssignments, out IEnumerable<Exception> fundAssignmentExceptions))
            {
                exceptions = exceptions.Concat(fundAssignmentExceptions);
            }
        }
        return !exceptions.Any();
    }
}
