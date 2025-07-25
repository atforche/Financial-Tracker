namespace Domain.Funds;

/// <summary>
/// Value object class representing a Fund Amount.
/// A Fund Amount represents a monetary amount associated with a particular Fund.
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

    /// <summary>
    /// Builds a new Fund Amount with the sign of the Amount field flipped 
    /// </summary>
    internal FundAmount GetWithReversedAmount() => new()
    {
        FundId = FundId,
        Amount = Amount * -1
    };
}