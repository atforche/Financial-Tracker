namespace Domain.Transactions.UpdateRequests;

/// <summary>
/// Record representing a request to update a <see cref="TransferTransaction"/>
/// </summary>
public record UpdateTransferTransactionRequest : UpdateTransactionRequest
{
    /// <inheritdoc/>
    public override required DateOnly TransactionDate { get; init; }

    /// <inheritdoc/>
    public override required string Location { get; init; }

    /// <inheritdoc/>
    public override required string Description { get; init; }

    /// <summary>
    /// Posted Date for the Debit Account of this Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Transfer Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }
}
