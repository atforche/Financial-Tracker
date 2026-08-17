namespace Domain.Locations.Queries;

/// <summary>
/// Criteria for querying Locations.
/// </summary>
public sealed record LocationQuery(LocationFilter Filter, LocationSort Sort, int Offset, int? Limit);

/// <summary>
/// Criteria for filtering Locations.
/// </summary>
public sealed record LocationFilter(string? NameSearch, IReadOnlyCollection<Guid> Ids);

/// <summary>
/// Supported Location sort orders.
/// </summary>
public enum LocationSort
{
    /// <summary>
    /// Sorts by name ascending.
    /// </summary>
    Name,

    /// <summary>
    /// Sorts by name descending.
    /// </summary>
    NameDescending,
}