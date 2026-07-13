using Domain.AccountingPeriods;
using Domain.Validation;
using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Service for managing Transactions
/// </summary>
public abstract class TransactionService(
    AccountBalanceService accountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Accounting Period Repository
    /// </summary>
    protected IAccountingPeriodRepository AccountingPeriodRepository => accountingPeriodRepository;

    /// <summary>
    /// Transaction Repository
    /// </summary>
    protected ITransactionRepository TransactionRepository => transactionRepository;

    /// <summary>
    /// Validates a request to create a new Transaction
    /// </summary>
    protected bool ValidateCreate(
        CreateTransactionRequest request,
        Account? sourceAccount,
        ValidationErrorPath sourceAccountPath,
        IReadOnlyCollection<Account?> destinationAccounts,
        Func<int, ValidationErrorPath> destinationAccountsPathBuilder,
        IReadOnlyCollection<Fund> sourceFunds,
        Func<int, ValidationErrorPath> sourceFundsPathBuilder,
        IReadOnlyCollection<IReadOnlyCollection<Fund>> destinationFunds,
        Func<int, int, ValidationErrorPath> destinationFundsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        AccountingPeriod accountingPeriod = AccountingPeriodRepository.GetById(request.AccountingPeriodId);
        if (!ValidateAccountingPeriod(
                sourceAccount,
                sourceAccountPath,
                destinationAccounts,
                destinationAccountsPathBuilder,
                sourceFunds,
                sourceFundsPathBuilder,
                destinationFunds,
                destinationFundsPathBuilder,
                accountingPeriod,
                new ValidationErrorPath(nameof(CreateTransactionRequest.AccountingPeriodId)),
                out IEnumerable<ValidationError> accountingPeriodExceptions))
        {
            exceptions = exceptions.Concat(accountingPeriodExceptions);
        }
        if (!ValidateDate(
                accountingPeriod,
                new ValidationErrorPath(nameof(CreateTransactionRequest.AccountingPeriodId)),
                sourceAccount,
                sourceAccountPath,
                destinationAccounts,
                destinationAccountsPathBuilder,
                request.TransactionDate,
                new ValidationErrorPath(nameof(CreateTransactionRequest.TransactionDate)),
                out IEnumerable<ValidationError> dateExceptions))
        {
            exceptions = exceptions.Concat(dateExceptions);
        }
        if (!ValidateAmount(
                request.Amount,
                new ValidationErrorPath(nameof(CreateTransactionRequest.Amount)),
                out IEnumerable<ValidationError> amountExceptions))
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
        AddTransactionToBalanceHistories(transaction);
        transactionRepository.Add(transaction);
    }

    /// <summary>
    /// Validates a request to update an existing Transaction
    /// </summary>
    protected bool ValidateUpdate(
        Transaction transaction,
        UpdateTransactionRequest request,
        Account? sourceAccount,
        ValidationErrorPath sourceAccountPath,
        IReadOnlyCollection<Account?> destinationAccounts,
        Func<int, ValidationErrorPath> destinationAccountsPathBuilder,
        IReadOnlyCollection<Fund> sourceFunds,
        Func<int, ValidationErrorPath> sourceFundsPathBuilder,
        IReadOnlyCollection<IReadOnlyCollection<Fund>> destinationFunds,
        Func<int, int, ValidationErrorPath> destinationFundsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        AccountingPeriod accountingPeriod = AccountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        if (!ValidateAccountingPeriod(
                sourceAccount,
                sourceAccountPath,
                destinationAccounts,
                destinationAccountsPathBuilder,
                sourceFunds,
                sourceFundsPathBuilder,
                destinationFunds,
                destinationFundsPathBuilder,
                accountingPeriod,
                ValidationErrorPath.Empty,
                out IEnumerable<ValidationError> accountingPeriodExceptions))
        {
            exceptions = exceptions.Concat(accountingPeriodExceptions);
        }
        if (!ValidateDate(
                accountingPeriod,
                ValidationErrorPath.Empty,
                sourceAccount,
                sourceAccountPath,
                destinationAccounts,
                destinationAccountsPathBuilder,
                request.TransactionDate,
                new ValidationErrorPath(nameof(UpdateTransactionRequest.TransactionDate)),
                out IEnumerable<ValidationError> dateExceptions))
        {
            exceptions = exceptions.Concat(dateExceptions);
        }
        if (!ValidateAmount(request.Amount, new ValidationErrorPath(nameof(UpdateTransactionRequest.Amount)), out IEnumerable<ValidationError> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Updates the properties of an existing Transaction based on an UpdateTransactionRequest
    /// </summary>
    protected void UpdateTransaction(
        Transaction transaction,
        UpdateTransactionRequest request,
        Action? updateAdditionalProperties = null)
    {
        RemoveTransactionFromBalanceHistories(transaction);

        DateOnly oldDate = transaction.Date;
        transaction.Date = request.TransactionDate;
        if (oldDate != request.TransactionDate)
        {
            transaction.Sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        }
        transaction.Description = request.Description;
        transaction.Amount = request.Amount;
        updateAdditionalProperties?.Invoke();

        AddTransactionToBalanceHistories(transaction);
    }

    /// <summary>
    /// Validates the posting of this Transaction within an Account
    /// </summary>
    protected bool ValidatePosting(
        Transaction transaction,
        PostTransactionRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        var accountPath = new ValidationErrorPath(nameof(PostTransactionRequest.AccountId));
        var postedDatePath = new ValidationErrorPath(nameof(PostTransactionRequest.PostedDate));
        if (!transaction.GetAllAffectedAccountIds().Contains(request.AccountId))
        {
            exceptions = exceptions.Append(new ValidationError(accountPath, "The provided account is not associated with this transaction."));
            return false;
        }
        if (transaction.GetPostedDateForAccount(request.AccountId) != null)
        {
            exceptions = exceptions.Append(new ValidationError(accountPath, "The Transaction has already been posted to this Account."));
            return !exceptions.Any();
        }
        if (request.PostedDate < transaction.Date)
        {
            exceptions = exceptions.Append(new ValidationError(postedDatePath, "The provided date is earlier than the transaction date."));
        }
        AccountingPeriod accountingPeriod = AccountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        int monthDifference = Math.Abs(((accountingPeriod.Year - request.PostedDate.Year) * 12) + accountingPeriod.Month - request.PostedDate.Month);
        if (monthDifference > 1)
        {
            exceptions = exceptions.Append(new ValidationError(postedDatePath, "The provided date is not within the transaction's accounting period."));
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
    }

    /// <summary>
    /// Validates the unposting of this Transaction
    /// </summary>
    protected bool ValidateUnposting(Transaction transaction, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!AccountingPeriodRepository.GetById(transaction.AccountingPeriodId).IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "The Transaction's Accounting Period is closed."));
        }
        if (transaction.GetAllAffectedAccountIds().All(id => transaction.GetPostedDateForAccount(id) == null))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "The Transaction has not been posted to either account."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Unposts a Transaction from all accounts
    /// </summary>
    protected void UnpostTransaction(Transaction transaction)
    {
        accountingPeriodBalanceService.UnpostTransaction(transaction);
        accountBalanceService.UnpostTransaction(transaction);
        fundBalanceService.UnpostTransaction(transaction);
    }

    /// <summary>
    /// Validates the deletion of this Transaction
    /// </summary>
    protected bool ValidateDelete(Transaction transaction, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!AccountingPeriodRepository.GetById(transaction.AccountingPeriodId).IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "The provided transaction is within a closed accounting period."));
        }
        if (transaction.GetAllAffectedAccountIds().Any(id => transaction.GetPostedDateForAccount(id) != null))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "The Transaction has been posted and cannot be deleted."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Deletes a Transaction
    /// </summary>
    protected void DeleteTransaction(Transaction transaction)
    {
        RemoveTransactionFromBalanceHistories(transaction);
        TransactionRepository.Delete(transaction);
    }

    /// <summary>
    /// Adds a Transaction's effects to all balance histories.
    /// </summary>
    private void AddTransactionToBalanceHistories(Transaction transaction)
    {
        accountingPeriodBalanceService.AddTransaction(transaction);
        accountBalanceService.AddTransaction(transaction);
        fundBalanceService.AddTransaction(transaction);
    }

    /// <summary>
    /// Removes a Transaction's effects from all balance histories.
    /// </summary>
    private void RemoveTransactionFromBalanceHistories(Transaction transaction)
    {
        accountingPeriodBalanceService.DeleteTransaction(transaction);
        accountBalanceService.DeleteTransaction(transaction);
        fundBalanceService.DeleteTransaction(transaction);
    }

    /// <summary>
    /// Validates the posted date for the provided accounting period, account, and date
    /// </summary>
    protected static bool ValidatePostedDate(
        AccountingPeriod accountingPeriod,
        Account account,
        DateOnly? postedDate,
        ValidationErrorPath postedDatePath,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (postedDate.HasValue)
        {
            if (!accountingPeriod.IsDateInPeriod(postedDate.Value))
            {
                exceptions = exceptions.Append(new ValidationError(postedDatePath, "Debit Posted Date must be within the Accounting Period"));
            }
            if (account.DateOpened < postedDate)
            {
                exceptions = exceptions.Append(new ValidationError(postedDatePath, "Debit Posted Date cannot be before the Transaction was added"));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the fund assignments for a Transaction at the supplied request path.
    /// </summary>
    protected virtual bool ValidateFundAssignments(
        decimal amount,
        ValidationErrorPath amountPath,
        IReadOnlyCollection<FundAmount> fundAssignments,
        Func<int, ValidationErrorPath> fundAssignmentsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        foreach ((int index, FundAmount fundAmount) in fundAssignments.Index())
        {
            if (fundAmount.Amount <= 0)
            {
                exceptions = exceptions.Append(new ValidationError(
                    fundAssignmentsPathBuilder(index).Append(nameof(FundAmount.Amount)),
                    "Fund assignment amounts must be positive"));
            }
            if (fundAssignments.Index().Any(pair => pair.Item.FundId == fundAmount.FundId && pair.Index != index))
            {
                exceptions = exceptions.Append(new ValidationError(
                    fundAssignmentsPathBuilder(index).Append(nameof(FundAmount.FundId)),
                    "Duplicate fund assignments are not allowed"));
            }
            if (Math.Round(fundAssignments.Sum(fundAmount => fundAmount.Amount), 2) > amount)
            {
                exceptions = exceptions.Append(new ValidationError(
                    fundAssignmentsPathBuilder(index).Append(nameof(FundAmount.Amount)),
                    "Sum of fund assignment amounts cannot exceed total transaction amount"));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Accounting Period for this Transaction
    /// </summary>
    private bool ValidateAccountingPeriod(
        Account? sourceAccount,
        ValidationErrorPath sourceAccountPath,
        IReadOnlyCollection<Account?> destinationAccounts,
        Func<int, ValidationErrorPath> destinationAccountsPathBuilder,
        IReadOnlyCollection<Fund> sourceFunds,
        Func<int, ValidationErrorPath> sourceFundsPathBuilder,
        IReadOnlyCollection<IReadOnlyCollection<Fund>> destinationFunds,
        Func<int, int, ValidationErrorPath> destinationFundsPathBuilder,
        AccountingPeriod accountingPeriod,
        ValidationErrorPath accountingPeriodPath,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!accountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(accountingPeriodPath, "The Accounting Period is closed."));
        }
        if (!ValidateAccountForAccountingPeriod(sourceAccount, sourceAccountPath, accountingPeriod, accountingPeriodPath, out IEnumerable<ValidationError> sourceAccountValidationExceptions))
        {
            exceptions = exceptions.Concat(sourceAccountValidationExceptions);
        }
        foreach ((int index, Account? account) in destinationAccounts.Index())
        {
            if (!ValidateAccountForAccountingPeriod(account, destinationAccountsPathBuilder(index), accountingPeriod, accountingPeriodPath, out IEnumerable<ValidationError> destinationAccountValidationExceptions))
            {
                exceptions = exceptions.Concat(destinationAccountValidationExceptions);
            }
        }
        foreach ((int index, Fund fund) in sourceFunds.Index())
        {
            if (!ValidateFundForAccountingPeriod(fund, sourceFundsPathBuilder(index), accountingPeriod, accountingPeriodPath, out IEnumerable<ValidationError> sourceFundValidationExceptions))
            {
                exceptions = exceptions.Concat(sourceFundValidationExceptions);
            }
        }
        foreach ((int index, IReadOnlyCollection<Fund> funds) in destinationFunds.Index())
        {
            foreach ((int fundIndex, Fund fund) in funds.Index())
            {
                if (!ValidateFundForAccountingPeriod(fund, destinationFundsPathBuilder(index, fundIndex), accountingPeriod, accountingPeriodPath, out IEnumerable<ValidationError> destinationFundValidationExceptions))
                {
                    exceptions = exceptions.Concat(destinationFundValidationExceptions);
                }
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates that an account can be used within the specified accounting period.
    /// </summary>
    private bool ValidateAccountForAccountingPeriod(Account? account, ValidationErrorPath accountPath, AccountingPeriod accountingPeriod, ValidationErrorPath accountingPeriodPath, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (account?.OpeningAccountingPeriodId == null)
        {
            return !exceptions.Any();
        }
        AccountingPeriod accountInitialPeriod = AccountingPeriodRepository.GetById(account.OpeningAccountingPeriodId);
        if (accountingPeriod.PeriodStartDate < accountInitialPeriod.PeriodStartDate)
        {
            exceptions = exceptions.Append(new ValidationError(accountPath, $"Account {account.Name} did not exist during the provided Accounting Period."));
            exceptions = exceptions.Append(new ValidationError(accountingPeriodPath, $"Account {account.Name} did not exist during the provided Accounting Period."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates that a fund can be used within the specified accounting period.
    /// </summary>
    private bool ValidateFundForAccountingPeriod(Fund? fund, ValidationErrorPath fundPath, AccountingPeriod accountingPeriod, ValidationErrorPath accountingPeriodPath, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (fund?.OpeningAccountingPeriodId == null)
        {
            return !exceptions.Any();
        }
        AccountingPeriod fundInitialPeriod = AccountingPeriodRepository.GetById(fund.OpeningAccountingPeriodId);
        if (accountingPeriod.PeriodStartDate < fundInitialPeriod.PeriodStartDate)
        {
            exceptions = exceptions.Append(new ValidationError(fundPath, $"Fund {fund.Name} did not exist during the provided Accounting Period."));
            exceptions = exceptions.Append(new ValidationError(accountingPeriodPath, $"Fund {fund.Name} did not exist during the provided Accounting Period."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Date for this Transaction
    /// </summary>
    private static bool ValidateDate(
        AccountingPeriod accountingPeriod,
        ValidationErrorPath accountingPeriodPath,
        Account? sourceAccount,
        ValidationErrorPath sourceAccountPath,
        IEnumerable<Account?> destinationAccounts,
        Func<int, ValidationErrorPath> destinationAccountsPathBuilder,
        DateOnly date,
        ValidationErrorPath datePath,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (date == DateOnly.MinValue)
        {
            exceptions = exceptions.Append(new ValidationError(datePath, "The provided date is blank."));
        }
        if (!accountingPeriod.IsDateInPeriod(date))
        {
            exceptions = exceptions.Append(new ValidationError(datePath, "The provided date is not within the transaction's accounting period."));
            exceptions = exceptions.Append(new ValidationError(accountingPeriodPath, "The provided date is not within the transaction's accounting period."));
        }
        if (!ValidateAccountForDate(sourceAccount, sourceAccountPath, date, datePath, out IEnumerable<ValidationError> sourceAccountExceptions))
        {
            exceptions = exceptions.Concat(sourceAccountExceptions);
        }
        foreach ((int index, Account? account) in destinationAccounts.Index())
        {
            if (!ValidateAccountForDate(account, destinationAccountsPathBuilder(index), date, datePath, out IEnumerable<ValidationError> destinationAccountExceptions))
            {
                exceptions = exceptions.Concat(destinationAccountExceptions);
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates that the provided date is not before the account was created.
    /// </summary>
    private static bool ValidateAccountForDate(Account? account, ValidationErrorPath accountPath, DateOnly date, ValidationErrorPath datePath, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (account == null)
        {
            return true;
        }
        if (date < account.DateOpened)
        {
            exceptions = exceptions.Append(new ValidationError(accountPath, $"The provided date is before the account {account.Name} was created."));
            exceptions = exceptions.Append(new ValidationError(datePath, $"The provided date is before the account {account.Name} was created."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the Amount for this Transaction
    /// </summary>
    private static bool ValidateAmount(decimal amount, ValidationErrorPath amountPath, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (amount <= 0)
        {
            exceptions = exceptions.Append(new ValidationError(amountPath, "The provided amount must be greater than zero."));
        }
        return !exceptions.Any();
    }
}
