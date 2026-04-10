using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.CreateRequests;

/// <summary>
/// Record representing a request to create a <see cref="SpendingTransaction"/>
/// </summary>
public record CreateSpendingTransactionRequest : CreateTransactionRequest
{
    /// <inheritdoc/>
    public override required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <inheritdoc/>
    public override required DateOnly TransactionDate { get; init; }

    /// <inheritdoc/>
    public override required string Location { get; init; }

    /// <inheritdoc/>
    public override required string Description { get; init; }

    /// <inheritdoc/>
    public override required decimal Amount { get; init; }

    /// <summary>
    /// Account for this Spending Transaction
    /// </summary>
    public required Account Account { get; init; }

    /// <summary>
    /// Posted Date for this Spending Transaction
    /// </summary>
    public DateOnly? PostedDate { get; init; }

    /// <summary>
    /// Fund Assignments for this Spending Transaction
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAssignments { get; init; }

    /// <summary>
    /// True if this transaction is the initial transaction for the account, false otherwise
    /// </summary>
    public required bool IsInitialTransactionForAccount { get; init; }
}
