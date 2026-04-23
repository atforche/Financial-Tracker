using Domain.Funds;

namespace Domain.Transactions.Spending;

/// <summary>
/// Record representing a request to update a <see cref="SpendingTransaction"/>
/// </summary>
public record UpdateSpendingTransactionRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Posted Date for the Debit Account of this Spending Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Spending Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }

    /// <summary>
    /// Fund Assignments for this Spending Transaction
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAssignments { get; init; }
}