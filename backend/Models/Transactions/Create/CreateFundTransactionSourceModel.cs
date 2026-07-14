namespace Models.Transactions.Create;

/// <summary>
/// Model representing the source of a fund transaction create request.
/// </summary>
public sealed class CreateFundTransactionSourceModel
{
    /// <summary>
    /// Fund ID for the source fund.
    /// </summary>
    public required Guid FundId { get; init; }
}