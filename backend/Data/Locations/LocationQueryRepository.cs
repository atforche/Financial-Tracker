using Domain;
using Domain.Locations;
using Domain.Locations.Queries;
using Microsoft.EntityFrameworkCore;

namespace Data.Locations;

/// <summary>
/// EF Core query repository for Locations.
/// </summary>
public sealed class LocationQueryRepository(DatabaseContext databaseContext) : ILocationQueryRepository
{
    /// <inheritdoc/>
    public async Task<QueryPage<Location>> GetAsync(LocationQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<Location> locations = databaseContext.Locations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Filter.NameSearch))
        {
            locations = locations.Where(location => EF.Functions.Like(location.Name, $"%{query.Filter.NameSearch}%"));
        }
        if (query.Filter.Ids.Count > 0)
        {
            var ids = query.Filter.Ids.Select(id => new LocationId(id)).ToList();
            locations = locations.Where(location => ids.Contains(location.Id));
        }
        locations = query.Sort == LocationSort.NameDescending
            ? locations.OrderByDescending(location => location.Name).ThenBy(location => location.Id)
            : locations.OrderBy(location => location.Name).ThenBy(location => location.Id);
        int totalCount = await locations.CountAsync(cancellationToken);
        locations = locations.Skip(query.Offset);
        if (query.Limit != null)
        {
            locations = locations.Take(query.Limit.Value);
        }
        return new QueryPage<Location>(await locations.ToListAsync(cancellationToken), totalCount);
    }
}