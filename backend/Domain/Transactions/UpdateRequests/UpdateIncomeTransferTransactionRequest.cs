namespace Domain.Transactions.UpdateRequests;

/// <summary>
/// Record representing a request to update an <see cref="IncomeTransferTransaction"/>
/// </summary>
public record UpdateIncomeTransferTransactionRequest : UpdateIncomeTransactionRequest
{
    /// <summary>
    /// Posted Date for the Debit Account of this Income Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }
}
