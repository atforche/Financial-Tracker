using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Service for managing Transactions
/// </summary>
public abstract class TransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    FundGoalService fundGoalService,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Validates a request to create a new Transaction
    /// </summary>
    protected bool ValidateCreate(
        CreateTransactionRequest request,
        IReadOnlyCollection<Account> accounts,
        IReadOnlyCollection<Fund> funds,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(request.AccountingPeriodId);
        if (!ValidateAccountingPeriod(accounts, funds, accountingPeriod, out IEnumerable<Exception> accountingPeriodExceptions))
        {
            exceptions = exceptions.Concat(accountingPeriodExceptions);
        }
        if (!ValidateDate(accountingPeriod, accounts, request.TransactionDate, out IEnumerable<Exception> dateExceptions))
        {
            exceptions = exceptions.Concat(dateExceptions);
        }
        if (!ValidateAmount(request.Amount, out IEnumerable<Exception> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Adds a Transaction to the appropriate services and repositories
    /// </summary>
    protected void AddTransaction(Transaction transaction)
    {
        accountBalanceService.AddTransaction(transaction);
        fundBalanceService.AddTransaction(transaction);
        fundGoalService.AddTransaction(transaction);
        transactionRepository.Add(transaction);
    }

    /// <summary>
    /// Validates a request to update an existing Transaction
    /// </summary>
    protected bool ValidateUpdate(
        Transaction transaction,
        UpdateTransactionRequest request,
        IReadOnlyCollection<Account> accounts,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        if (!ValidateDate(accountingPeriod, accounts, request.TransactionDate, out IEnumerable<Exception> dateExceptions))
        {
            exceptions = exceptions.Concat(dateExceptions);
        }
        if (!ValidateAmount(request.Amount, out IEnumerable<Exception> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Updates the properties of an existing Transaction based on an UpdateTransactionRequest
    /// </summary>
    protected void UpdateTransaction(Transaction transaction, UpdateTransactionRequest request)
    {
        transaction.Date = request.TransactionDate;
        transaction.Location = request.Location;
        transaction.Description = request.Description;
        transaction.Amount = request.Amount;

        accountBalanceService.UpdateTransaction(transaction);
        fundBalanceService.UpdateTransaction(transaction);
        fundGoalService.UpdateTransaction(transaction);
    }

    /// <summary>
    /// Validates the posting of this Transaction within an Account
    /// </summary>
    protected bool ValidatePosting(
        Transaction transaction,
        AccountId accountId,
        DateOnly postedDate,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!transaction.GetAllAffectedAccountIds().Contains(accountId))
        {
            exceptions = exceptions.Append(new InvalidAccountException("The provided account is not associated with this transaction."));
            return false;
        }
        if (transaction.GetPostedDateForAccount(accountId) != null)
        {
            exceptions = exceptions.Append(new UnableToPostException("The Transaction has already been posted to this Account."));
            return !exceptions.Any();
        }
        if (postedDate < transaction.Date)
        {
            exceptions = exceptions.Append(new InvalidDateException("The provided date is earlier than the transaction date."));
        }
        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        int monthDifference = Math.Abs(((accountingPeriod.Year - postedDate.Year) * 12) + accountingPeriod.Month - postedDate.Month);
        if (monthDifference > 1)
        {
            exceptions = exceptions.Append(new InvalidDateException("The provided date is not within the transaction's accounting period."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Posts a Transaction to an Account
    /// </summary>
    protected void PostTransaction(Transaction transaction, AccountId accountId)
    {
        accountingPeriodBalanceService.PostTransaction(transaction, accountId);
        accountBalanceService.PostTransaction(transaction, accountId);
        fundBalanceService.PostTransaction(transaction, accountId);
        fundGoalService.PostTransaction(transaction, accountId);
    }

    /// <summary>
    /// Validates the posted date for the provided accounting period, account, and date
    /// </summary>
    protected static bool ValidatePostedDate(AccountingPeriod accountingPeriod, Account account, DateOnly? postedDate, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (postedDate.HasValue)
        {
            if (!accountingPeriod.IsDateInPeriod(postedDate.Value))
            {
                exceptions = exceptions.Append(new InvalidDateException("Debit Posted Date must be within the Accounting Period"));
            }
            if (account.AddDate < postedDate)
            {
                exceptions = exceptions.Append(new InvalidDateException("Debit Posted Date cannot be before the Transaction was added"));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the fund assignments for a Transaction
    /// </summary>
    protected virtual bool ValidateFundAssignments(decimal amount, IReadOnlyCollection<FundAmount> fundAssignments, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (fundAssignments.Any(fundAmount => fundAmount.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidFundAmountException("Fund assignment amounts must be positive"));
        }
        if (fundAssignments.Select(fundAmount => fundAmount.FundId).Distinct().Count() != fundAssignments.Count)
        {
            exceptions = exceptions.Append(new InvalidFundAmountException("Duplicate Funds are not allowed in fund assignments"));
        }
        if (fundAssignments.Sum(fundAmount => fundAmount.Amount) > amount)
        {
            exceptions = exceptions.Append(new InvalidFundAmountException("Sum of fund assignment amounts cannot exceed total transaction amount"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the IsInitialTransactionForAccount property for a Transaction
    /// </summary>
    protected static bool ValidateInitialTransactionForAccount(bool isInitialTransactionForAccount, Account account, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (isInitialTransactionForAccount && account.InitialTransaction != null)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Initial transaction for account already exists"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Accounting Period for this Transaction
    /// </summary>
    private bool ValidateAccountingPeriod(IReadOnlyCollection<Account> accounts, IReadOnlyCollection<Fund> funds, AccountingPeriod accountingPeriod, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The Accounting Period is closed."));
        }
        foreach (Account account in accounts)
        {
            AccountingPeriod accountInitialPeriod = accountingPeriodRepository.GetById(account.AddAccountingPeriodId);
            if (accountingPeriod.PeriodStartDate < accountInitialPeriod.PeriodStartDate)
            {
                exceptions = exceptions.Append(new InvalidAccountingPeriodException($"Account {account.Name} did not exist during the provided Accounting Period."));
            }
        }
        foreach (Fund fund in funds)
        {
            AccountingPeriod fundInitialPeriod = accountingPeriodRepository.GetById(fund.AddAccountingPeriodId);
            if (accountingPeriod.PeriodStartDate < fundInitialPeriod.PeriodStartDate)
            {
                exceptions = exceptions.Append(new InvalidAccountingPeriodException($"Fund {fund.Name} did not exist during the provided Accounting Period."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Date for this Transaction
    /// </summary>
    private static bool ValidateDate(
        AccountingPeriod accountingPeriod,
        IEnumerable<Account> accounts,
        DateOnly date,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (date == DateOnly.MinValue)
        {
            exceptions = exceptions.Append(new InvalidDateException("The provided date is blank."));
        }
        if (!accountingPeriod.IsDateInPeriod(date))
        {
            exceptions = exceptions.Append(new InvalidDateException("The provided date is not within the transaction's accounting period."));
        }
        foreach (Account account in accounts)
        {
            if (date < account.AddDate)
            {
                exceptions = exceptions.Append(new InvalidDateException($"The provided date is before the account {account.Name} was created."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Amount for this Transaction
    /// </summary>
    private static bool ValidateAmount(decimal amount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (amount <= 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException("The provided amount must be greater than zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Attempts to unpost an existing Transaction within an Account
    /// </summary>
    public bool TryUnpost(Transaction transaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateUnposting(transaction, out IEnumerable<Exception> unpostingExceptions))
        {
            exceptions = exceptions.Concat(unpostingExceptions);
            return false;
        }
        accountingPeriodBalanceService.UnpostTransaction(transaction);
        accountBalanceService.UnpostTransaction(transaction);
        fundBalanceService.UnpostTransaction(transaction);
        fundGoalService.UnpostTransaction(transaction);
        foreach (AccountId accountId in transaction.GetAllAffectedAccountIds())
        {
            ClearPostedDate(transaction, accountId);
        }
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Transaction
    /// </summary>
    public bool TryDelete(Transaction transaction, AccountId? accountBeingDeleted, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        AccountId? generatedByAccountId = GetGeneratedByAccountId(transaction);
        if (accountBeingDeleted != generatedByAccountId && generatedByAccountId != null)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("The provided transaction was auto-generated by an account."));
        }
        if (!accountingPeriodRepository.GetById(transaction.AccountingPeriodId).IsOpen)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("The provided transaction is within a closed accounting period."));
        }
        if (!ValidateNotPosted(transaction, accountBeingDeleted))
        {
            exceptions = exceptions.Append(new UnableToDeleteException("The provided transaction has already been posted to an account."));
        }
        if (exceptions.Any())
        {
            return false;
        }
        accountBalanceService.DeleteTransaction(transaction);
        fundBalanceService.DeleteTransaction(transaction);
        fundGoalService.DeleteTransaction(transaction);
        transactionRepository.Delete(transaction);
        return true;
    }

    /// <summary>
    /// Validates that the Transaction has not been posted to either Account
    /// </summary>
    private static bool ValidateNotPosted(Transaction transaction, AccountId? accountBeingDeleted)
    {
        foreach (AccountId accountId in transaction.GetAllAffectedAccountIds())
        {
            if (accountId != accountBeingDeleted && transaction.GetPostedDateForAccount(accountId) != null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Validates the unposting of this Transaction
    /// </summary>
    private bool ValidateUnposting(Transaction transaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!accountingPeriodRepository.GetById(transaction.AccountingPeriodId).IsOpen)
        {
            exceptions = exceptions.Append(new UnableToUnpostException("The Transaction's Accounting Period is closed."));
        }
        if (transaction.GetAllAffectedAccountIds().All(id => transaction.GetPostedDateForAccount(id) == null))
        {
            exceptions = exceptions.Append(new UnableToUnpostException("The Transaction has not been posted to either account."));
        }
        if (GetGeneratedByAccountId(transaction) != null)
        {
            exceptions = exceptions.Append(new UnableToUnpostException("The provided transaction was auto-generated by an account."));
        }
        return !exceptions.Any();
    }
}