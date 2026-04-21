namespace Domain.Transactions.UpdateRequests;

/// <summary>
/// Record representing a request to update a <see cref="AccountTransferTransaction"/>
/// </summary>
public record UpdateAccountTransferTransactionRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Posted Date for the Debit Account of this Account Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Account Transfer Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }
}