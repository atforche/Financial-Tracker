namespace Domain.Aggregates;

/// <summary>
/// Interface shared by all Aggregate Repositories
/// </summary>
public interface IAggregateRepository<TAggregate>
{
    /// <summary>
    /// Finds the Aggregate with the specified external ID.
    /// </summary>
    /// <param name="id">ID of the Aggregate to find</param>
    /// <returns>The Aggregate that was found, or null if one wasn't found</returns>
    TAggregate? FindByExternalIdOrNull(Guid id);
}