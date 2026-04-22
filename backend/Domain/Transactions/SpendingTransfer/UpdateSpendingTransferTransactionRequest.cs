using Domain.Transactions.Spending;

namespace Domain.Transactions.SpendingTransfer;

/// <summary>
/// Record representing a request to update a <see cref="SpendingTransferTransaction"/>
/// </summary>
public record UpdateSpendingTransferTransactionRequest : UpdateSpendingTransactionRequest
{
    /// <summary>
    /// Posted Date for the Credit Account of this Spending Transfer Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }
}