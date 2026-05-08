namespace Models.Funds;

/// <summary>
/// Model representing a request to update a Fund
/// </summary>
public class UpdateFundModel
{
    /// <summary>
    /// Name for the Fund
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description for the Fund
    /// </summary>
    public required string Description { get; init; }
}