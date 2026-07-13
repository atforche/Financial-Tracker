namespace Domain.Funds;

/// <summary>
/// Value object class representing an amount associated with a particular Fund.
/// </summary>
public class FundAmount
{
    /// <summary>
    /// Fund ID for this Fund Amount
    /// </summary>
    public required FundId FundId { get; init; }

    /// <summary>
    /// Amount for this Fund Amount
    /// </summary>
    public required decimal Amount { get; init; }
}