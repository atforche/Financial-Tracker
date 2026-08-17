namespace Models.Locations;

/// <summary>
/// API model for a Location.
/// </summary>
public class LocationModel
{
    /// <summary>
    /// Location ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Canonical Location name.
    /// </summary>
    public required string Name { get; init; }
}