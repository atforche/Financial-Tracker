namespace Models.Funds;

/// <summary>
/// Model representing a Fund
/// </summary>
public class FundModel
{
    /// <summary>
    /// ID for the Fund
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name for the Fund
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description for the Fund
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Current Balance for the Fund
    /// </summary>
    public required FundBalanceModel CurrentBalance { get; init; }
}