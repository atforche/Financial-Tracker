namespace Models.Transactions.Update;

/// <summary>
/// Model representing the source of a spending transaction update request.
/// </summary>
public sealed class UpdateSpendingTransactionSourceModel
{
    /// <summary>
    /// Account ID for the source account.
    /// </summary>
    public required Guid AccountId { get; init; }
}
