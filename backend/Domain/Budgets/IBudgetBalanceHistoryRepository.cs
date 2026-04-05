namespace Domain.Budgets;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="BudgetBalanceHistory"/>
/// </summary>
public interface IBudgetBalanceHistoryRepository
{
    /// <summary>
    /// Adds the provided Budget Balance History to the repository
    /// </summary>
    void Add(BudgetBalanceHistory budgetBalanceHistory);

    /// <summary>
    /// Deletes the provided Budget Balance History from the repository
    /// </summary>
    void Delete(BudgetBalanceHistory budgetBalanceHistory);
}
