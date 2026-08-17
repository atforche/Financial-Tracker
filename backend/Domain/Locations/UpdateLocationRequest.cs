namespace Domain.Locations;

/// <summary>
/// Request to rename a Location.
/// </summary>
public sealed class UpdateLocationRequest
{
    /// <summary>
    /// New canonical display name for the Location.
    /// </summary>
    public required string Name { get; init; }
}