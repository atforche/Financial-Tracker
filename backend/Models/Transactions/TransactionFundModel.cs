namespace Models.Transactions;

/// <summary>
/// Model representing a fund referenced by a transaction.
/// </summary>
public class TransactionFundModel
{
    /// <summary>
    /// Fund ID.
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Fund name.
    /// </summary>
    public required string FundName { get; init; }
}