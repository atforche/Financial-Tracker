using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.Spending;

/// <summary>
/// Record representing a request to create a <see cref="SpendingTransaction"/>
/// </summary>
public record CreateSpendingTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Debit Account for this Spending Transaction
    /// </summary>
    public required Account DebitAccount { get; init; }

    /// <summary>
    /// Posted Date for the Debit Account of this Spending Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Credit Account for this Spending Transaction
    /// </summary>
    public required Account? CreditAccount { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Spending Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }

    /// <summary>
    /// Fund Assignments for this Spending Transaction
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAssignments { get; init; }
}