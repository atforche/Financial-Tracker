using Domain.Accounts;
using Domain.Transactions.Spending;

namespace Domain.Transactions.SpendingTransfer;

/// <summary>
/// Record representing a request to create a <see cref="SpendingTransferTransaction"/>
/// </summary>
public record CreateSpendingTransferTransactionRequest : CreateSpendingTransactionRequest
{
    /// <summary>
    /// Credit Account for this Spending Transfer Transaction
    /// </summary>
    public required Account CreditAccount { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Spending Transfer Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }
}