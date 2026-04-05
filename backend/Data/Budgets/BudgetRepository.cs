using System.Diagnostics.CodeAnalysis;
using Domain.Budgets;

namespace Data.Budgets;

/// <summary>
/// Repository that allows Budgets to be persisted to the database
/// </summary>
public class BudgetRepository(DatabaseContext databaseContext) : IBudgetRepository
{
    #region IBudgetRepository

    /// <inheritdoc/>
    public IReadOnlyCollection<Budget> GetAll() => databaseContext.Budgets.ToList();

    /// <inheritdoc/>
    public Budget GetById(BudgetId id) => databaseContext.Budgets.Single(budget => budget.Id == id);

    /// <inheritdoc/>
    public bool TryGetByName(string name, out Budget? budget)
    {
        budget = databaseContext.Budgets.FirstOrDefault(b => b.Name == name);
        return budget != null;
    }

    /// <inheritdoc/>
    public void Add(Budget budget) => databaseContext.Add(budget);

    /// <inheritdoc/>
    public void Delete(Budget budget) => databaseContext.Remove(budget);

    #endregion

    /// <summary>
    /// Attempts to get the Budget with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Budget? budget)
    {
        budget = databaseContext.Budgets.FirstOrDefault(budget => ((Guid)(object)budget.Id) == id);
        return budget != null;
    }
}
