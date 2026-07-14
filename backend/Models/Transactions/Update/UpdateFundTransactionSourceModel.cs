namespace Models.Transactions.Update;

/// <summary>
/// Model representing the source of a fund transaction update request.
/// </summary>
public sealed class UpdateFundTransactionSourceModel
{
    /// <summary>
    /// Fund ID for the source fund.
    /// </summary>
    public required Guid FundId { get; init; }
}