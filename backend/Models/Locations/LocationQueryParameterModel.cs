namespace Models.Locations;

/// <summary>
/// Query parameters for retrieving Locations.
/// </summary>
public sealed class LocationQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filters applied to Locations.
    /// </summary>
    public LocationFilterModel? Filter { get; init; }

    /// <summary>
    /// Sort order for Locations.
    /// </summary>
    public LocationSortModel? Sort { get; init; }
}