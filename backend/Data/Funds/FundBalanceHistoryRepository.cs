using Domain.Funds;
using Domain.Transactions;

namespace Data.Funds;

/// <summary>
/// Repository that allows Fund Balance Histories to be persisted to the database
/// </summary>
public class FundBalanceHistoryRepository(DatabaseContext databaseContext) : IFundBalanceHistoryRepository
{
    /// <inheritdoc/>
    public FundBalanceHistory? GetLatestForFund(FundId fundId) =>
        MergeWithTracked(
            databaseContext.FundBalanceHistories.Where(history => history.Fund.Id == fundId),
            history => history.Fund.Id == fundId)
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundBalanceHistory> GetAllByTransactionId(TransactionId transactionId) =>
        MergeWithTracked(
            databaseContext.FundBalanceHistories.Where(history => history.TransactionId == transactionId),
            history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ToList();

    /// <inheritdoc/>
    public FundBalanceHistory GetEarliestByTransactionId(FundId fundId, TransactionId transactionId) =>
        MergeWithTracked(
            databaseContext.FundBalanceHistories.Where(history => history.Fund.Id == fundId && history.TransactionId == transactionId),
            history => history.Fund.Id == fundId && history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .First();

    /// <inheritdoc/>
    public FundBalanceHistory? GetLatestHistoryEarlierThan(FundId fundId, DateOnly historyDate, int sequenceNumber) =>
        MergeWithTracked(
            databaseContext.FundBalanceHistories.Where(history => history.Fund.Id == fundId &&
                (history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequenceNumber))),
            history => history.Fund.Id == fundId &&
                (history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequenceNumber)))
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundBalanceHistory> GetAllHistoriesLaterThan(FundId fundId, DateOnly historyDate, int sequence) =>
        MergeWithTracked(
            databaseContext.FundBalanceHistories.Where(history => history.Fund.Id == fundId &&
                (history.Date > historyDate || (history.Date == historyDate && history.Sequence > sequence))),
            history => history.Fund.Id == fundId &&
                (history.Date > historyDate || (history.Date == historyDate && history.Sequence > sequence)))
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public void Add(FundBalanceHistory fundBalanceHistory) => databaseContext.Add(fundBalanceHistory);

    /// <inheritdoc/>
    public void Delete(FundBalanceHistory fundBalanceHistory) => databaseContext.Remove(fundBalanceHistory);

    /// <summary>
    /// Merges database results with non-deleted tracked histories so a transaction update observes its in-unit-of-work changes.
    /// </summary>
    private IEnumerable<FundBalanceHistory> MergeWithTracked(
        IQueryable<FundBalanceHistory> databaseQuery,
        Func<FundBalanceHistory, bool> predicate)
    {
        var entries = databaseContext.ChangeTracker.Entries<FundBalanceHistory>().ToList();
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
