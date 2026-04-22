using System.Diagnostics.CodeAnalysis;
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
    IAccountRepository accountRepository,
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
        if (!ValidateAmount(request, out IEnumerable<Exception> amountExceptions))
        {
            exceptions = exceptions.Concat(amountExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the posted date for the provided accounting period, account, and date
    /// </summary>
    protected bool ValidatePostedDate(CreateTransactionRequest request, Account account, DateOnly postedDate, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(request.AccountingPeriodId);
        if (!accountingPeriod.IsDateInPeriod(postedDate))
        {
            exceptions = exceptions.Append(new InvalidDateException("Debit Posted Date must be within the Accounting Period"));
        }
        if (account.AddDate < postedDate)
        {
            exceptions = exceptions.Append(new InvalidDateException("Debit Posted Date cannot be before the Transaction was added"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the fund assignments for a Transaction
    /// </summary>
    protected static bool ValidateFundAssignments(CreateTransactionRequest request, IReadOnlyCollection<FundAmount> fundAssignments, out IEnumerable<Exception> exceptions)
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
        if (fundAssignments.Sum(fundAmount => fundAmount.Amount) > request.Amount)
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
    private static bool ValidateAmount(CreateTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.Amount <= 0)
        {
            exceptions = exceptions.Append(new InvalidAmountException("The provided amount must be greater than zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Attempts to update an existing Transaction
    /// </summary>
    public bool TryUpdate(Transaction transaction, UpdateTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (GetGeneratedByAccountId(transaction) != null)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("The provided transaction was auto-generated by an account and cannot be updated."));
        }
        if (!ValidateNotPosted(transaction, null))
        {
            exceptions = exceptions.Append(new UnableToUpdateException("The provided transaction has already been posted to an account."));
        }
        IEnumerable<Account> accounts = transaction.GetAllAffectedAccountIds().Select(accountRepository.GetById);
        if (!ValidateDate(accountingPeriodRepository.GetById(transaction.AccountingPeriodId), accounts, request.TransactionDate, out IEnumerable<Exception> dateExceptions))
        {
            exceptions = exceptions.Concat(dateExceptions);
        }
        if (!ValidateFundAmountsForUpdate(request, out IEnumerable<Exception> fundAmountExceptions))
        {
            exceptions = exceptions.Concat(fundAmountExceptions);
        }
        if (exceptions.Any())
        {
            return false;
        }
        transaction.Date = request.TransactionDate;
        transaction.Location = request.Location;
        transaction.Description = request.Description;
        if (transaction is SpendingTransaction spendingTransaction && request is UpdateSpendingTransactionRequest spendingRequest)
        {
            spendingTransaction.UpdateFundAssignments(spendingRequest.FundAssignments);
        }
        else if (transaction is IncomeTransaction incomeTransaction && request is UpdateIncomeTransactionRequest incomeRequest)
        {
            incomeTransaction.UpdateFundAssignments(incomeRequest.FundAssignments);
        }
        accountBalanceService.UpdateTransaction(transaction);
        fundBalanceService.UpdateTransaction(transaction);
        fundGoalService.UpdateTransaction(transaction);
        foreach ((AccountId accountId, DateOnly postedDate) in GetUpdatePostings(transaction, request))
        {
            if (!TryPost(transaction, accountId, postedDate, out IEnumerable<Exception> postingExceptions))
            {
                exceptions = exceptions.Concat(postingExceptions);
            }
        }
        if (exceptions.Any())
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Transaction within an Account
    /// </summary>
    public bool TryPost(Transaction transaction, AccountId account, DateOnly postedDate, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidatePosting(transaction, account, postedDate, out IEnumerable<Exception> postingExceptions))
        {
            exceptions = exceptions.Concat(postingExceptions);
            return false;
        }
        SetPostedDate(transaction, account, postedDate);
        accountingPeriodBalanceService.PostTransaction(transaction, account);
        accountBalanceService.PostTransaction(transaction, account);
        fundBalanceService.PostTransaction(transaction, account);
        fundGoalService.PostTransaction(transaction, account);
        return true;
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
    /// Gets the AccountId that generated this transaction, or null if not auto-generated
    /// </summary>
    private static AccountId? GetGeneratedByAccountId(Transaction transaction) =>
        transaction is IncomeTransaction income ? income.GeneratedByAccountId : null;

    /// <summary>
    /// Sets the posted date on the appropriate property of the transaction
    /// </summary>
    private static void SetPostedDate(Transaction transaction, AccountId account, DateOnly postedDate)
    {
        switch (transaction)
        {
            case SpendingTransferTransaction spendingTransfer when spendingTransfer.CreditAccountId == account:
                spendingTransfer.CreditPostedDate = postedDate;
                break;
            case SpendingTransaction spending:
                spending.PostedDate = postedDate;
                break;
            case IncomeTransferTransaction incomeTransfer when incomeTransfer.DebitAccountId == account:
                incomeTransfer.DebitPostedDate = postedDate;
                break;
            case IncomeTransaction income:
                income.PostedDate = postedDate;
                break;
            case AccountTransferTransaction transfer when transfer.DebitAccountId == account:
                transfer.DebitPostedDate = postedDate;
                break;
            case AccountTransferTransaction transfer:
                transfer.CreditPostedDate = postedDate;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Clears the posted date on the appropriate property of the transaction
    /// </summary>
    private static void ClearPostedDate(Transaction transaction, AccountId account)
    {
        switch (transaction)
        {
            case SpendingTransferTransaction spendingTransfer when spendingTransfer.CreditAccountId == account:
                spendingTransfer.CreditPostedDate = null;
                break;
            case SpendingTransaction spending:
                spending.PostedDate = null;
                break;
            case IncomeTransferTransaction incomeTransfer when incomeTransfer.DebitAccountId == account:
                incomeTransfer.DebitPostedDate = null;
                break;
            case IncomeTransaction income:
                income.PostedDate = null;
                break;
            case AccountTransferTransaction transfer when transfer.DebitAccountId == account:
                transfer.DebitPostedDate = null;
                break;
            case AccountTransferTransaction transfer:
                transfer.CreditPostedDate = null;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Validates the fund amounts for an updated Transaction
    /// </summary>
    private static bool ValidateFundAmountsForUpdate(UpdateTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request is UpdateSpendingTransactionRequest spendingRequest)
        {
            if (spendingRequest.FundAssignments.Count == 0)
            {
                exceptions = exceptions.Append(new InvalidFundAmountException("The provided list of fund amounts is empty."));
            }
            if (spendingRequest.FundAssignments.Any(fundAmount => fundAmount.Amount <= 0))
            {
                exceptions = exceptions.Append(new InvalidFundAmountException("The provided fund amounts must be greater than zero."));
            }
        }
        else if (request is UpdateIncomeTransactionRequest incomeRequest && incomeRequest.FundAssignments.Any(fundAmount => fundAmount.Amount <= 0))
        {
            exceptions = exceptions.Append(new InvalidFundAmountException("The provided fund amounts must be greater than zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the posting of this Transaction within an Account
    /// </summary>
    private bool ValidatePosting(
        Transaction transaction,
        AccountId account,
        DateOnly postedDate,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!transaction.GetAllAffectedAccountIds().Contains(account))
        {
            exceptions = exceptions.Append(new InvalidAccountException("The provided account is not associated with this transaction."));
            return false;
        }
        if (transaction.GetPostedDateForAccount(account) != null)
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