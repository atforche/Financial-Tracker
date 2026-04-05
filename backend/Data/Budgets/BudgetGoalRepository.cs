using Domain.AccountingPeriods;
using Domain.Budgets;

namespace Data.Budgets;

/// <summary>
/// Repository that allows Budget Goals to be persisted to the database
/// </summary>
public class BudgetGoalRepository(DatabaseContext databaseContext) : IBudgetGoalRepository
{
    #region IBudgetGoalRepository

    /// <inheritdoc/>
    public BudgetGoal GetById(BudgetGoalId id) => databaseContext.BudgetGoals.Single(budgetGoal => budgetGoal.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<BudgetGoal> GetAllByBudget(BudgetId budgetId) =>
        databaseContext.BudgetGoals.Where(budgetGoal => budgetGoal.BudgetId == budgetId).ToList();

    /// <inheritdoc/>
    public BudgetGoal? GetByBudgetAndAccountingPeriod(BudgetId budgetId, AccountingPeriodId accountingPeriodId) =>
        databaseContext.BudgetGoals.FirstOrDefault(budgetGoal =>
            budgetGoal.BudgetId == budgetId && budgetGoal.AccountingPeriodId == accountingPeriodId);

    /// <inheritdoc/>
    public void Add(BudgetGoal budgetGoal) => databaseContext.Add(budgetGoal);

    /// <inheritdoc/>
    public void Delete(BudgetGoal budgetGoal) => databaseContext.Remove(budgetGoal);

    #endregion
}
