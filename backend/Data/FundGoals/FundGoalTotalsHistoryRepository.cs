using Domain.AccountingPeriods;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Transactions;

namespace Data.FundGoals;

/// <summary>
/// Repository that persists Fund Goal totals history.
/// </summary>
public sealed class FundGoalTotalsHistoryRepository(DatabaseContext databaseContext) : IFundGoalTotalsHistoryRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<FundGoalTotalsHistory> GetAllByTransaction(TransactionId transactionId) =>
        MergeWithTracked(
            databaseContext.FundGoalTotalsHistories.Where(history => history.TransactionId == transactionId),
            history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public FundGoalTotalsHistory? GetLatestEarlierThan(
        FundId fundId,
        AccountingPeriodId accountingPeriodId,
        DateOnly historyDate,
        int sequence) => MergeWithTracked(
            Query(fundId, accountingPeriodId)
                .Where(history => history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequence)),
            history => history.FundId == fundId && history.AccountingPeriodId == accountingPeriodId &&
                (history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequence)))
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundGoalTotalsHistory> GetAllLaterThan(
        FundId fundId,
        AccountingPeriodId accountingPeriodId,
        DateOnly historyDate,
        int sequence) => MergeWithTracked(
            Query(fundId, accountingPeriodId)
                .Where(history => history.Date > historyDate || (history.Date == historyDate && history.Sequence > sequence)),
            history => history.FundId == fundId && history.AccountingPeriodId == accountingPeriodId &&
                (history.Date > historyDate || (history.Date == historyDate && history.Sequence > sequence)))
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public void Add(FundGoalTotalsHistory history) => databaseContext.Add(history);

    /// <inheritdoc/>
    public void Delete(FundGoalTotalsHistory history) => databaseContext.Remove(history);

    private IQueryable<FundGoalTotalsHistory> Query(FundId fundId, AccountingPeriodId accountingPeriodId) =>
        databaseContext.FundGoalTotalsHistories.Where(history =>
            history.FundId == fundId && history.AccountingPeriodId == accountingPeriodId);

    /// <summary>
    /// Merges database results with non-deleted tracked histories so a transaction update observes its in-unit-of-work changes.
    /// </summary>
    private IEnumerable<FundGoalTotalsHistory> MergeWithTracked(
        IQueryable<FundGoalTotalsHistory> databaseQuery,
        Func<FundGoalTotalsHistory, bool> predicate)
    {
        var entries = databaseContext.ChangeTracker.Entries<FundGoalTotalsHistory>().ToList();
        var deletedHistoryIds = entries
            .Where(entry => entry.State == Microsoft.EntityFrameworkCore.EntityState.Deleted)
            .Select(entry => entry.Entity.Id)
            .ToHashSet();
        var trackedHistories = entries
            .Where(entry => entry.State != Microsoft.EntityFrameworkCore.EntityState.Deleted && predicate(entry.Entity))
            .Select(entry => entry.Entity)
            .ToDictionary(history => history.Id);
        return databaseQuery.ToList()
            .Where(history => !deletedHistoryIds.Contains(history.Id) && !trackedHistories.ContainsKey(history.Id))
            .Concat(trackedHistories.Values);
    }
}