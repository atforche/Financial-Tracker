using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.Income;

/// <summary>
/// Record representing a request to create an <see cref="IncomeTransaction"/>
/// </summary>
public record CreateIncomeTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Credit Account for this Income Transaction
    /// </summary>
    public required Account CreditAccount { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Income Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }

    /// <summary>
    /// Debit Account for this Income Transaction
    /// </summary>
    public required Account? DebitAccount { get; init; }

    /// <summary>
    /// Posted Date for the Debit Account of this Income Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Fund assignments for the amount of this Income Transaction
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAssignments { get; init; }
}