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
    /// Attempts to create a new Spending Transaction
    /// </summary>
    public bool TryCreate(
        CreateSpendingTransactionRequest request,
        [NotNullWhen(true)] out SpendingTransaction? transaction,
        out IEnumerable<Exception> exceptions)
    {
        transaction = null;

        if (!ValidateCreate(
                request,
                new List<Account?> { request.DebitAccount, request.CreditAccount }.OfType<Account>().ToList(),
                out exceptions))
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
        Account debitAccount = accountRepository.GetById(transaction.DebitAccountId);
        Account? creditAccount = transaction.CreditAccountId != null ? accountRepository.GetById(transaction.CreditAccountId) : null;
        if (!ValidateUpdate(
                transaction,
                request,
                new List<Account?> { debitAccount, creditAccount }.OfType<Account>().ToList(),
                out exceptions))
        {
            return false;
        }
        transaction.UpdateFundAssignments(request.FundAssignments);
        UpdateTransaction(transaction, request);
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
        if (accountId == transaction.DebitAccountId)
        {
            transaction.DebitPostedDate = postedDate;
        }
        else if (accountId == transaction.CreditAccountId)
        {
            transaction.CreditPostedDate = postedDate;
        }
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
        transaction.DebitPostedDate = null;
        transaction.CreditPostedDate = null;
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
            request.FundAssignments.Select(fundAmount => fundRepository.GetById(fundAmount.FundId)).ToList(),
            out exceptions);

        if (!ValidateAccount(request, out IEnumerable<Exception> accountExceptions))
        {
            exceptions = exceptions.Concat(accountExceptions);
        }
        if (!ValidateFundAssignments(request.Amount, request.FundAssignments, out IEnumerable<Exception> fundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(fundAssignmentExceptions);
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
        _ = base.ValidateUpdate(transaction, request, accounts, out exceptions);

        if (transaction.DebitPostedDate.HasValue || transaction.CreditPostedDate.HasValue)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("Transaction has already been posted and cannot be updated"));
        }
        if (!ValidateFundAssignments(request.Amount, request.FundAssignments, out IEnumerable<Exception> fundAssignmentExceptions))
        {
            exceptions = exceptions.Concat(fundAssignmentExceptions);
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
    private static bool ValidateAccount(CreateSpendingTransactionRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!request.DebitAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions must debit a tracked account"));
        }
        if (request.CreditAccount?.Id == request.DebitAccount.Id)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Debit and Credit Accounts cannot be the same"));
        }
        if (request.CreditAccount != null && request.CreditAccount.Type.IsTracked())
        {
            exceptions = exceptions.Append(new InvalidAccountException("Spending Transactions cannot credit a tracked account"));
        }
        return !exceptions.Any();
    }
}