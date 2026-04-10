namespace Domain.Transactions.UpdateRequests;

/// <summary>
/// Abstract record representing a request to update a <see cref="Transaction"/>
/// </summary>
public abstract record UpdateTransactionRequest
{
    /// <summary>
    /// Date for the Transaction
    /// </summary>
    public abstract DateOnly TransactionDate { get; init; }

    /// <summary>
    /// Location for the Transaction
    /// </summary>
    public abstract string Location { get; init; }

    /// <summary>
    /// Description for the Transaction
    /// </summary>
    public abstract string Description { get; init; }
}
