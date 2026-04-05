namespace Domain.Budgets;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Budget"/>
/// </summary>
public interface IBudgetRepository
{
    /// <summary>
    /// Gets all Budgets in the repository.
    /// </summary>
    IReadOnlyCollection<Budget> GetAll();

    /// <summary>
    /// Gets the Budget with the specified ID.
    /// </summary>
    Budget GetById(BudgetId id);

    /// <summary>
    /// Attempts to get the Budget with the specified name
    /// </summary>
    bool TryGetByName(string name, out Budget? budget);

    /// <summary>
    /// Adds the provided Budget to the repository
    /// </summary>
    void Add(Budget budget);

    /// <summary>
    /// Deletes the provided Budget from the repository
    /// </summary>
    void Delete(Budget budget);
}
