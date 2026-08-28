namespace Models.Transactions.Update;

/// <summary>
/// Model representing the destination of a refund transaction update request.
/// </summary>
public sealed class UpdateRefundTransactionDestinationModel
{
    /// <summary>
    /// Account ID for the destination account.
    /// </summary>
    public required Guid AccountId { get; init; }
}
