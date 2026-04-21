using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.CreateRequests;

/// <summary>
/// Record representing a request to create a <see cref="SpendingTransaction"/>
/// </summary>
public record CreateSpendingTransactionRequest : CreateTransactionRequest
{
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