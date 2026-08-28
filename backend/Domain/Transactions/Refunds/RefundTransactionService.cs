using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Validation;

namespace Domain.Transactions.Refunds;

/// <summary>
/// Service for managing Refund Transactions.
/// </summary>
public class RefundTransactionService(
    AccountBalanceService accountBalanceService,
    PendingAccountBalanceService pendingAccountBalanceService,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundBalanceService fundBalanceService,
    PendingFundBalanceService pendingFundBalanceService,
    FundGoalTotalsHistoryService fundGoalTotalsHistoryService,
    PendingFundGoalTotalsService pendingFundGoalTotalsService,
    IAccountingPeriodRepository accountingPeriodRepository,
    IFundRepository fundRepository,
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
    /// Attempts to create a new Refund Transaction.
    /// </summary>
    public bool TryCreate(
        CreateRefundTransactionRequest request,
        [NotNullWhen(true)] out RefundTransaction? transaction,
        out IEnumerable<ValidationError> exceptions)
    {
        transaction = null;
        _ = ValidateCreate(
            request,
            request.Sources.Select(source => source.Account).ToList(),
            request.Destination.Account,
            request.Sources.Select(source => source.FundAssignments
                .Select(assignment => fundRepository.GetById(assignment.FundId))
                .ToList()).ToList(),
            out exceptions);
        exceptions = exceptions.Concat(ValidateStructure(request.Amount, request.Sources, request.Destination, null));
        if (exceptions.Any())
        {
            return false;
        }
        int sequence = TransactionRepository.GetNextSequenceForDate(request.TransactionDate);
        transaction = new RefundTransaction(request, sequence);
        AddTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Refund Transaction.
    /// </summary>
    public bool TryUpdate(
        RefundTransaction transaction,
        UpdateRefundTransactionRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        _ = ValidateUpdate(
            transaction,
            request,
            null,
            ValidationErrorPath.Empty,
            request.Sources.Select(source => source.Account).Append(request.Destination.Account).ToList(),
            (i) => ValidationErrorPath.Empty,
            [],
            (i) => ValidationErrorPath.Empty,
            request.Sources.Select(source => source.FundAssignments
                .Select(assignment => fundRepository.GetById(assignment.FundId))
                .ToList()).ToList(),
            (i, j) => ValidationErrorPath.Empty,
            out exceptions);
        exceptions = exceptions.Concat(ValidateStructure(request.Amount, request.Sources, request.Destination, transaction));
        if (exceptions.Any())
        {
            return false;
        }
        UpdateTransaction(
            transaction,
            request,
            () =>
            {
                transaction.UpdateRefundSources(request.Sources);
                transaction.UpdateRefundDestination(request.Destination);
            });
        return true;
    }

    /// <summary>
    /// Attempts to post an existing Refund Transaction to a specific Account.
    /// </summary>
    public bool TryPost(RefundTransaction transaction, PostTransactionRequest request, out IEnumerable<ValidationError> exceptions)
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
    /// Attempts to unpost an existing Refund Transaction.
    /// </summary>
    public bool TryUnpost(RefundTransaction transaction, out IEnumerable<ValidationError> exceptions)
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
    /// Attempts to delete an existing Refund Transaction.
    /// </summary>
    public bool TryDelete(RefundTransaction transaction, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateDelete(transaction, out exceptions))
        {
            return false;
        }
        DeleteTransaction(transaction);
        return true;
    }

    /// <summary>
    /// Validates a request to create a new Refund Transaction.
    /// </summary>
    private bool ValidateCreate(
        CreateRefundTransactionRequest request,
        IReadOnlyCollection<Account?> sources,
        Account destination,
        IReadOnlyCollection<IReadOnlyCollection<Fund>> funds,
        out IEnumerable<ValidationError> exceptions) =>
        ValidateCreate(
            request,
            null,
            ValidationErrorPath.Empty,
            sources.Append(destination).ToList(),
            (i) => ValidationErrorPath.Empty,
            [],
            (i) => ValidationErrorPath.Empty,
            funds,
            (i, j) => ValidationErrorPath.Empty,
            out exceptions);

    /// <summary>
    /// Validates the structure of this Refund Transaction.
    /// </summary>
    private List<ValidationError> ValidateStructure(
        decimal amount,
        IReadOnlyCollection<RefundTransactionSource> sources,
        RefundTransactionDestination destination,
        RefundTransaction? existing)
    {
        List<ValidationError> errors = [];
        if (sources.Count == 0)
        {
            errors.Add(new ValidationError(new ValidationErrorPath("Sources", 0), "Refund Transactions must have at least one source"));
        }
        if (!destination.Account.Type.IsTracked())
        {
            errors.Add(new ValidationError(new ValidationErrorPath("Destination.Account"), "Refund Transactions must credit a tracked account"));
        }
        if (destination.PostedDate != null || sources.Any(source => source.PostedDate != null))
        {
            errors.Add(new ValidationError(ValidationErrorPath.Empty, "Posted dates cannot be set directly"));
        }
        if (existing != null && existing.GetAllAffectedAccountIds().Any(id => existing.GetPostedDateForAccount(id) != null))
        {
            errors.Add(new ValidationError(ValidationErrorPath.Empty, "Transaction has already been posted and cannot be updated"));
        }
        foreach ((int index, RefundTransactionSource source) in sources.Index())
        {
            ValidationErrorPath path = new("Sources", index);
            if (source.Account == null == (source.Location == null))
            {
                errors.Add(new ValidationError(path, "Refund sources must have either an account or a location"));
            }
            if (source.Account?.Type.IsTracked() == true)
            {
                errors.Add(new ValidationError(path.Append("Account"), "Refund Transactions cannot source money from a tracked account"));
            }
            if (source.Account?.Id == destination.Account.Id)
            {
                errors.Add(new ValidationError(path.Append("Account"), "Source and destination accounts cannot be the same"));
            }
            if (source.Amount <= 0)
            {
                errors.Add(new ValidationError(path.Append("Amount"), "Refund source amounts must be positive"));
            }
            if (sources.Index().Any(pair => pair.Index != index &&
                ((source.Account != null && pair.Item.Account?.Id == source.Account.Id) ||
                 (source.Location != null && pair.Item.Location?.Id == source.Location.Id))))
            {
                errors.Add(new ValidationError(path, "Duplicate refund sources are not allowed"));
            }
            _ = ValidateFundAssignments(
                source.Amount,
                path.Append("Amount"),
                source.FundAssignments,
                (i) => path.AppendWithIndex("FundAssignments", i),
                out IEnumerable<ValidationError> assignmentErrors);
            errors.AddRange(assignmentErrors);
        }
        if (Math.Round(sources.Sum(source => source.Amount), 2) != Math.Round(amount, 2))
        {
            errors.Add(new ValidationError(new ValidationErrorPath("Amount"), "Refund source amounts must equal the transaction amount"));
        }
        return errors;
    }

    /// <summary>
    /// Validates the Fund Assignments for this Refund Transaction.
    /// </summary>
    protected override bool ValidateFundAssignments(
        decimal amount,
        ValidationErrorPath amountPath,
        IReadOnlyCollection<FundAmount> fundAssignments,
        Func<int, ValidationErrorPath> fundAssignmentsPathBuilder,
        out IEnumerable<ValidationError> exceptions)
    {
        _ = base.ValidateFundAssignments(amount, amountPath, fundAssignments, fundAssignmentsPathBuilder, out exceptions);
        if (fundAssignments.Any(assignment => assignment.FundId == Fund.UnassignedFundId))
        {
            exceptions = exceptions.Append(new ValidationError(amountPath, "Cannot refund money to the unassigned fund"));
        }
        if (fundAssignments.Sum(assignment => assignment.Amount) != amount)
        {
            exceptions = exceptions.Append(new ValidationError(amountPath, "Total amount assigned to funds must equal the source amount"));
        }
        return !exceptions.Any();
    }
}
