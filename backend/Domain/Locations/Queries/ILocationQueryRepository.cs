namespace Domain.Locations.Queries;

/// <summary>
/// Read operations for Locations.
/// </summary>
public interface ILocationQueryRepository
{
    /// <summary>
    /// Gets a page of Locations matching the query.
    /// </summary>
    Task<QueryPage<Location>> GetAsync(LocationQuery query, CancellationToken cancellationToken = default);
}