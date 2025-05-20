using Domain;

namespace Data.Repositories;

/// <summary>
/// Base class shared by all Aggregate Repositories
/// </summary>
public abstract class AggregateRepository<TAggregate>(DatabaseContext databaseContext) where TAggregate : Entity
{
    /// <summary>
    /// Database Context for this Aggregate Repository
    /// </summary>
    protected DatabaseContext DatabaseContext { get; } = databaseContext;

    /// <summary>
    /// Finds the Aggregate with the provided ID
    /// </summary>
    /// <param name="id">ID of the Aggregate to find</param>
    /// <returns>The Aggregate that was found or null if one isn't found</returns>
    public TAggregate? FindByIdOrNull(EntityId id) =>
        DatabaseContext.Set<TAggregate>().SingleOrDefault(aggregate => aggregate.Id == id);
}