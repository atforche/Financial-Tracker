namespace Models.Transactions.Update;

/// <summary>
/// Model representing a destination of a fund transaction update request.
/// </summary>
public sealed class UpdateFundTransactionDestinationModel
{
    /// <summary>
    /// Fund ID for the destination fund.
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Amount directed to this destination fund.
    /// </summary>
    public required decimal Amount { get; init; }
}