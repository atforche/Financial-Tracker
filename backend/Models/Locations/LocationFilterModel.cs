namespace Models.Locations;

/// <summary>
/// Filters applied when retrieving Locations.
/// </summary>
public sealed class LocationFilterModel
{
    /// <summary>
    /// Optional substring matched against Location names.
    /// </summary>
    public string? NameSearch { get; init; }

    /// <summary>
    /// Optional Location IDs to include.
    /// </summary>
    public IReadOnlyCollection<Guid>? Ids { get; init; }
}
