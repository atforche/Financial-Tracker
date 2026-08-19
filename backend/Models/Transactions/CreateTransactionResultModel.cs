namespace Models.Transactions;

/// <summary>
/// Model returned after creating a Transaction.
/// </summary>
public sealed class CreateTransactionResultModel
{
    /// <summary>
    /// Gets the ID of the created Transaction.
    /// </summary>
    public required Guid Id { get; init; }
}
