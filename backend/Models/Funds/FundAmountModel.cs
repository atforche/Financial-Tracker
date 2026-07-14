namespace Models.Funds;

/// <summary>
/// Model representing an amount associated with a particular Fund
/// </summary>
public class FundAmountModel
{
    /// <summary>
    /// Fund for this Fund Amount
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Amount for this Fund Amount
    /// </summary>
    public required decimal Amount { get; init; }
}