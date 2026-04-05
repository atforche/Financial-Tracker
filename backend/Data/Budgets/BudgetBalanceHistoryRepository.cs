using Domain.Budgets;
using Domain.Transactions;

namespace Data.Budgets;

/// <summary>
/// Repository that allows Budget Balance Histories to be persisted to the database
/// </summary>
public class BudgetBalanceHistoryRepository(DatabaseContext databaseContext) : IBudgetBalanceHistoryRepository
{
    #region IBudgetBalanceHistoryRepository

    /// <inheritdoc/>
    public int GetNextSequenceForBudgetGoalAndDate(BudgetGoalId budgetGoalId, DateOnly historyDate)
    {
        var historiesOnDate = databaseContext.BudgetBalanceHistories
            .Where(history => history.BudgetGoal.Id == budgetGoalId && history.Date == historyDate)
            .ToList();
        return historiesOnDate.Count == 0 ? 1 : historiesOnDate.Max(history => history.Sequence) + 1;
    }

    /// <inheritdoc/>
    public BudgetBalanceHistory? GetLatestForBudgetGoal(BudgetGoalId budgetGoalId) =>
        databaseContext.BudgetBalanceHistories
            .Where(history => history.BudgetGoal.Id == budgetGoalId)
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<BudgetBalanceHistory> GetAllByTransactionId(TransactionId transactionId) =>
        databaseContext.BudgetBalanceHistories
            .Where(history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public BudgetBalanceHistory GetEarliestByTransactionId(TransactionId transactionId) =>
        databaseContext.BudgetBalanceHistories
            .Where(history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .FirstOrDefault()
        ?? databaseContext.BudgetBalanceHistories.Local
            .Where(history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .First();

    /// <inheritdoc/>
    public BudgetBalanceHistory? GetLatestHistoryEarlierThan(BudgetGoalId budgetGoalId, DateOnly historyDate, int sequenceNumber) =>
        databaseContext.BudgetBalanceHistories
            .Where(history => history.BudgetGoal.Id == budgetGoalId &&
                              (history.Date < historyDate ||
                               (history.Date == historyDate && history.Sequence < sequenceNumber)))
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<(BudgetBalanceHistory History, Transaction Transaction)> GetAllHistoriesLaterThan(BudgetGoalId budgetGoalId, DateOnly historyDate, int sequence)
    {
        var results = databaseContext.BudgetBalanceHistories
            .Where(history => history.BudgetGoal.Id == budgetGoalId &&
                              (history.Date > historyDate ||
                               (history.Date == historyDate && history.Sequence > sequence)))
            .Join(databaseContext.Transactions,
                history => history.TransactionId,
                transaction => transaction.Id,
                (history, transaction) => new { history, transaction })
            .OrderBy(result => result.history.Date)
            .ThenBy(result => result.history.Sequence)
            .ToList();
        return results.Select(result => (result.history, result.transaction)).ToList();
    }

    /// <inheritdoc/>
    public void Add(BudgetBalanceHistory budgetBalanceHistory) => databaseContext.Add(budgetBalanceHistory);

    /// <inheritdoc/>
    public void Delete(BudgetBalanceHistory budgetBalanceHistory) => databaseContext.Remove(budgetBalanceHistory);

    #endregion
}
