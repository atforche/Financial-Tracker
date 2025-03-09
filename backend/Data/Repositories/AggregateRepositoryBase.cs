using Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

/// <summary>
/// Base class shared by all Aggregate Repositories
/// </summary>
public abstract class AggregateRepositoryBase<TAggregate>(DatabaseContext databaseContext) where TAggregate : EntityBase
{
    /// <summary>
    /// Database Context for this Aggregate Repository
    /// </summary>
    protected DatabaseContext DatabaseContext { get; } = databaseContext;

    /// <summary>
    /// Finds the Aggregate with the provided external ID
    /// </summary>
    /// <param name="id">External ID of the Aggregate to find</param>
    /// <returns>The Aggregate that was found. An exception is thrown if one isn't found</returns>
    public TAggregate FindByExternalId(Guid id) => FindByExternalIdOrNull(id) ?? throw new InvalidOperationException();

    /// <summary>
    /// Finds the Aggregate with the provided external ID
    /// </summary>
    /// <param name="id">External ID of the Aggregate to find</param>
    /// <returns>The Aggregate that was found or null if one isn't found</returns>
    public TAggregate? FindByExternalIdOrNull(Guid id) => DatabaseContext.Set<TAggregate>()
        .SingleOrDefault(aggregate => EF.Property<Guid>(aggregate, "_externalId") == id);
}