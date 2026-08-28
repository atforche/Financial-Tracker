namespace Models.Transactions.Create;

/// <summary>
/// Model representing the destination of a refund transaction create request.
/// </summary>
public sealed class CreateRefundTransactionDestinationModel
{
    /// <summary>
    /// Account ID for the destination account.
    /// </summary>
    public required Guid AccountId { get; init; }
}
