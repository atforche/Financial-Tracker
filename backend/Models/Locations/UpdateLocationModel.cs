namespace Models.Locations;

/// <summary>
/// API request to rename a Location.
/// </summary>
public sealed class UpdateLocationModel
{
    /// <summary>
    /// New canonical Location name.
    /// </summary>
    public required string Name { get; init; }
}