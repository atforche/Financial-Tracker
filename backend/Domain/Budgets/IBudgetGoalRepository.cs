namespace Domain.Budgets;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="BudgetGoal"/>
/// </summary>
public interface IBudgetGoalRepository
{
    /// <summary>
    /// Gets the Budget Goal with the specified ID.
    /// </summary>
    BudgetGoal GetById(BudgetGoalId id);

    /// <summary>
    /// Gets all Budget Goals associated with the specified Budget.
    /// </summary>
    IReadOnlyCollection<BudgetGoal> GetAllByBudget(BudgetId budgetId);

    /// <summary>
    /// Attempts to get the Budget Goal for the specified Budget and Accounting Period.
    /// </summary>
    BudgetGoal? GetByBudgetAndAccountingPeriod(BudgetId budgetId, AccountingPeriods.AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Adds the provided Budget Goal to the repository
    /// </summary>
    void Add(BudgetGoal budgetGoal);

    /// <summary>
    /// Deletes the provided Budget Goal from the repository
    /// </summary>
    void Delete(BudgetGoal budgetGoal);
}
