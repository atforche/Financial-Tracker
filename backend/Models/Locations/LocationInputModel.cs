namespace Models.Locations;

/// <summary>
/// Selects an existing Location or explicitly requests a new one.
/// </summary>
public sealed class LocationInputModel
{
    /// <summary>
    /// Existing Location ID.
    /// </summary>
    public Guid? LocationId { get; init; }

    /// <summary>
    /// Confirmed name for a new Location created with the parent operation.
    /// </summary>
    public string? NewLocationName { get; init; }
}