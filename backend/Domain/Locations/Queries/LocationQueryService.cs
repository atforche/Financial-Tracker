namespace Domain.Locations.Queries;

/// <summary>
/// Interprets Location queries.
/// </summary>
public sealed class LocationQueryService(ILocationQueryRepository locationQueryRepository)
{
    /// <summary>
    /// Gets Locations matching the provided query.
    /// </summary>
    public Task<QueryPage<Location>> GetAsync(LocationQuery query, CancellationToken cancellationToken = default) =>
        locationQueryRepository.GetAsync(query, cancellationToken);
}