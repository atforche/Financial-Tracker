using Domain.Funds;

namespace Domain.Transactions.UpdateRequests;

/// <summary>
/// Record representing a request to update a <see cref="SpendingTransaction"/>
/// </summary>
public record UpdateSpendingTransactionRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Posted Date for this Spending Transaction
    /// </summary>
    public DateOnly? PostedDate { get; init; }

    /// <summary>
    /// Fund Assignments for this Spending Transaction
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAssignments { get; init; }
}