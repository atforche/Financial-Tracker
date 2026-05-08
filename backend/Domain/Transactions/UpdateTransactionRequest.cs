namespace Domain.Transactions;

/// <summary>
/// Abstract record representing a request to update a <see cref="Transaction"/>
/// </summary>
public abstract record UpdateTransactionRequest
{
    /// <summary>
    /// Date for the Transaction
    /// </summary>
    public required DateOnly TransactionDate { get; init; }

    /// <summary>
    /// Location for the Transaction
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// Description for the Transaction
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount for the Transaction
    /// </summary>
    public required decimal Amount { get; init; }
}