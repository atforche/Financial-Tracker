namespace Models.Funds;

/// <summary>
/// Model representing an amount associated with a particular Fund
/// </summary>
public class FundAmountModel
{
    /// <summary>
    /// Fund for this Fund Amount
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Amount for this Fund Amount
    /// </summary>
    public required decimal Amount { get; init; }
}