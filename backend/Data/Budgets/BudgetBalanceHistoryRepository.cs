using Domain.Budgets;

namespace Data.Budgets;

/// <summary>
/// Repository that allows Budget Balance Histories to be persisted to the database
/// </summary>
public class BudgetBalanceHistoryRepository(DatabaseContext databaseContext) : IBudgetBalanceHistoryRepository
{
    #region IBudgetBalanceHistoryRepository

    /// <inheritdoc/>
    public void Add(BudgetBalanceHistory budgetBalanceHistory) => databaseContext.Add(budgetBalanceHistory);

    /// <inheritdoc/>
    public void Delete(BudgetBalanceHistory budgetBalanceHistory) => databaseContext.Remove(budgetBalanceHistory);

    #endregion
}
