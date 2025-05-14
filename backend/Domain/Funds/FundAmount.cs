namespace Domain.Funds;

/// <summary>
/// Value object class representing a Fund Amount.
/// A Fund Amount represents a monetary amount associated with a particular Fund.
/// </summary>
public class FundAmount
{
    /// <summary>
    /// Fund for this Fund Amount
    /// </summary>
    public required Fund Fund { get; init; }

    /// <summary>
    /// Amount for this Fund Amount
    /// </summary>
    public required decimal Amount { get; init; }
}