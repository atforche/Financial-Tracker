namespace Models.Transactions.Create;

/// <summary>
/// Model representing a destination of a fund transaction create request.
/// </summary>
public sealed class CreateFundTransactionDestinationModel
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