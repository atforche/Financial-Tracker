using Domain.Accounts;

namespace Domain.Transactions.CreateRequests;

/// <summary>
/// Record representing a request to create an <see cref="IncomeTransferTransaction"/>
/// </summary>
public record CreateIncomeTransferTransactionRequest : CreateIncomeTransactionRequest
{
    /// <summary>
    /// Debit Account for this Income Transfer Transaction
    /// </summary>
    public required Account DebitAccount { get; init; }

    /// <summary>
    /// Posted Date for the Debit Account of this Income Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }
}