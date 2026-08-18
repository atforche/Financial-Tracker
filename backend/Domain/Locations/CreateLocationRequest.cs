namespace Domain.Locations;

/// <summary>
/// Request to create a Location.
/// </summary>
public sealed class CreateLocationRequest
{
    /// <summary>
    /// Canonical display name for the Location.
    /// </summary>
    public required string Name { get; init; }
}
