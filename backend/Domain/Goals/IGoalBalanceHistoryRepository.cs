using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.Goals;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="GoalBalanceHistory"/>.
/// </summary>
public interface IGoalBalanceHistoryRepository
{
    /// <summary>
    /// Gets the latest Goal Balance History for a given Fund and Accounting Period, or null if none exists.
    /// </summary>
    GoalBalanceHistory? GetLatestForFundAndAccountingPeriod(FundId fundId, AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Gets all Goal Balance Histories for a given Transaction ID.
    /// </summary>
    IReadOnlyCollection<GoalBalanceHistory> GetAllByTransactionId(TransactionId transactionId);

    /// <summary>
    /// Gets the latest Goal Balance History for a given Fund and Accounting Period that is earlier than the specified date and sequence, or null if none exists.
    /// </summary>
    GoalBalanceHistory? GetLatestHistoryEarlierThan(FundId fundId, AccountingPeriodId accountingPeriodId, DateOnly historyDate, int sequence);

    /// <summary>
    /// Gets all Goal Balance Histories for a given Fund and Accounting Period that are later than the specified date and sequence.
    /// </summary>
    IReadOnlyCollection<GoalBalanceHistory> GetAllHistoriesLaterThan(FundId fundId, AccountingPeriodId accountingPeriodId, DateOnly historyDate, int sequence);

    /// <summary>
    /// Adds a Goal Balance History to the repository.
    /// </summary>
    void Add(GoalBalanceHistory goalBalanceHistory);

    /// <summary>
    /// Deletes a Goal Balance History from the repository.
    /// </summary>
    void Delete(GoalBalanceHistory goalBalanceHistory);
}