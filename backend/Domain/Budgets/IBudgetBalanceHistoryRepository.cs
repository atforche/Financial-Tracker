using Domain.Transactions;

namespace Domain.Budgets;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="BudgetBalanceHistory"/>
/// </summary>
public interface IBudgetBalanceHistoryRepository
{
    /// <summary>
    /// Gets the next sequence number for the specified Budget Goal ID and date
    /// </summary>
    int GetNextSequenceForBudgetGoalAndDate(BudgetGoalId budgetGoalId, DateOnly historyDate);

    /// <summary>
    /// Gets the latest Budget Balance History entry for the specified Budget Goal ID
    /// </summary>
    BudgetBalanceHistory? GetLatestForBudgetGoal(BudgetGoalId budgetGoalId);

    /// <summary>
    /// Gets all Budget Balance History entries for the specified Transaction ID
    /// </summary>
    IReadOnlyCollection<BudgetBalanceHistory> GetAllByTransactionId(TransactionId transactionId);

    /// <summary>
    /// Gets the earliest Budget Balance History entry for the specified Transaction ID
    /// </summary>
    BudgetBalanceHistory GetEarliestByTransactionId(TransactionId transactionId);

    /// <summary>
    /// Gets the latest Budget Balance History entry earlier than the specified date and sequence number
    /// </summary>
    BudgetBalanceHistory? GetLatestHistoryEarlierThan(BudgetGoalId budgetGoalId, DateOnly historyDate, int sequenceNumber);

    /// <summary>
    /// Gets all Budget Balance History entries later than the specified date and sequence number
    /// </summary>
    IReadOnlyCollection<(BudgetBalanceHistory History, Transaction Transaction)> GetAllHistoriesLaterThan(BudgetGoalId budgetGoalId, DateOnly historyDate, int sequence);

    /// <summary>
    /// Adds the provided Budget Balance History to the repository
    /// </summary>
    void Add(BudgetBalanceHistory budgetBalanceHistory);

    /// <summary>
    /// Deletes the provided Budget Balance History from the repository
    /// </summary>
    void Delete(BudgetBalanceHistory budgetBalanceHistory);
}
