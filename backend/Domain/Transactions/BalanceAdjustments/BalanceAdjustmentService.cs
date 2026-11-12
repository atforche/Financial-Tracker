using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Validation;

namespace Domain.Transactions.BalanceAdjustments;

/// <summary>
/// Service for managing Balance Adjustments
/// </summary>
/// <remarks>
/// A Balance Adjustment is always posted on its date, so creating one also posts it, and updating or deleting one
/// first removes its posted effects.
/// </remarks>
public class BalanceAdjustmentService(
    AccountBalanceService accountBalanceService,
    PendingAccountBalanceService pendingAccountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    PendingFundBalanceService pendingFundBalanceService,
    FundGoalTotalsHistoryService fundGoalTotalsHistoryService,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository) :
    TransactionService(
        accountBalanceService,
        pendingAccountBalanceService,
        accountingPeriodBalanceService,
        fundBalanceService,
        pendingFundBalanceService,
        fundGoalTotalsHistoryService,
        accountingPeriodRepository,
        transactionRepository)
{
    /// <inheritdoc/>
    protected override bool AllowsNegativeAmount => true;

    /// <summary>
    /// Attempts to create a new Balance Adjustment
    /// </summary>
    public bool TryCreate(
        CreateBalanceAdjustmentRequest request,
        [NotNullWhen(true)] out BalanceAdjustment? transaction,
        out IEnumerable<ValidationError> exceptions)
    {
        transaction = null;
        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new BalanceAdjustment(request, sequence);
        AddTransaction(transaction);
        PostTransaction(transaction, transaction.Account.Id);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Balance Adjustment
    /// </summary>
    public bool TryUpdate(
        BalanceAdjustment transaction,
        UpdateBalanceAdjustmentRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateUpdate(transaction, request, out exceptions))
        {
            return false;
        }
        UnpostTransaction(transaction, null);
        UpdateTransaction(transaction, request, () => transaction.UpdateAccount(request.Account));
        PostTransaction(transaction, transaction.Account.Id);
        return true;
    }

    /// <inheritdoc/>
    public override bool TryDelete(Transaction transaction, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];
        if (!AccountingPeriodRepository.GetById(transaction.AccountingPeriodId).IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "The provided transaction is within a closed accounting period."));
            return false;
        }
        UnpostTransaction(transaction, null);
        DeleteTransaction(transaction);
        return true;
    }

    private bool ValidateCreate(CreateBalanceAdjustmentRequest request, out IEnumerable<ValidationError> exceptions)
    {
        var accountPath = new ValidationErrorPath(nameof(CreateBalanceAdjustmentRequest.Account));
        _ = ValidateCreate(
            request,
            request.Account,
            accountPath,
            [],
            (i) => ValidationErrorPath.Empty,
            [],
            (i) => ValidationErrorPath.Empty,
            [],
            (i, j) => ValidationErrorPath.Empty,
            out exceptions);
        exceptions = exceptions.Concat(ValidateAccount(request.Account, accountPath));
        return !exceptions.Any();
    }

    private bool ValidateUpdate(
        BalanceAdjustment transaction,
        UpdateBalanceAdjustmentRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        var accountPath = new ValidationErrorPath(nameof(UpdateBalanceAdjustmentRequest.Account));
        _ = ValidateUpdate(
            transaction,
            request,
            request.Account,
            accountPath,
            [],
            (i) => ValidationErrorPath.Empty,
            [],
            (i) => ValidationErrorPath.Empty,
            [],
            (i, j) => ValidationErrorPath.Empty,
            out exceptions);
        exceptions = exceptions.Concat(ValidateAccount(request.Account, accountPath));
        return !exceptions.Any();
    }

    private static IEnumerable<ValidationError> ValidateAccount(Account account, ValidationErrorPath accountPath)
    {
        if (account.Type is not (AccountType.Investment or AccountType.Retirement or AccountType.Debt))
        {
            yield return new ValidationError(accountPath, "Balance adjustments can only be made to Investment, Retirement, or Debt accounts.");
        }
    }
}
