namespace Models.Locations;

/// <summary>
/// API request to create a Location.
/// </summary>
public sealed class CreateLocationModel
{
    /// <summary>
    /// Canonical Location name.
    /// </summary>
    public required string Name { get; init; }
}
