namespace Domain.Funds;

/// <summary>
/// Record representing a request to update a <see cref="Fund"/>
/// </summary>
public record UpdateFundRequest
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
