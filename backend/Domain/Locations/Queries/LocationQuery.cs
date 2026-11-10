namespace Domain.Locations.Queries;

/// <summary>
/// Criteria for querying Locations.
/// </summary>
public sealed record LocationQuery(LocationFilter Filter, int Offset, int? Limit);

/// <summary>
/// Criteria for filtering Locations.
/// </summary>
public sealed record LocationFilter(string? NameSearch, IReadOnlyCollection<Guid> Ids);
