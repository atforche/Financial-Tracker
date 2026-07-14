namespace Models.Transactions.Create;

/// <summary>
/// Model representing the source of a spending transaction create request.
/// </summary>
public sealed class CreateSpendingTransactionSourceModel
{
    /// <summary>
    /// Account ID for the source account.
    /// </summary>
    public required Guid AccountId { get; init; }
}