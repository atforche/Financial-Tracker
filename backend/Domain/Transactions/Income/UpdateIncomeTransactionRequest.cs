using Domain.Funds;

namespace Domain.Transactions.Income;

/// <summary>
/// Record representing a request to update an <see cref="IncomeTransaction"/>
/// </summary>
public record UpdateIncomeTransactionRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Credit Posted Date for this Income Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }

    /// <summary>
    /// Debit Posted Date for this Income Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Fund Assignments for this Income Transaction
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAssignments { get; init; }
}