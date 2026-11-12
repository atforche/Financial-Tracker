using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;

namespace Data.Accounts;

/// <summary>
/// Repository that allows Account Balance Histories to be persisted to the database
/// </summary>
public class AccountBalanceHistoryRepository(DatabaseContext databaseContext) : IAccountBalanceHistoryRepository
{
    /// <inheritdoc/>
    public AccountBalanceHistory? GetLatestPeriodHistoryEarlierThan(AccountId accountId, AccountingPeriodId accountingPeriodId, DateOnly historyDate, int sequence) =>
        MergeWithTracked(
            databaseContext.AccountBalanceHistories.Where(history => history.Account.Id == accountId
                && history.AccountingPeriodId == accountingPeriodId
                && (history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequence))),
            history => history.Account.Id == accountId && history.AccountingPeriodId == accountingPeriodId
                && (history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequence)))
            .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).FirstOrDefault();

    /// <inheritdoc/>
    public AccountBalanceHistory? GetLatestForAccount(AccountId accountId) =>
        MergeWithTracked(
            databaseContext.AccountBalanceHistories.Where(history => history.Account.Id == accountId),
            history => history.Account.Id == accountId)
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountBalanceHistory> GetAllByTransactionId(TransactionId transactionId) =>
        MergeWithTracked(
            databaseContext.AccountBalanceHistories.Where(history => history.TransactionId == transactionId),
            history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountBalanceHistory> GetAllByTransactionIdAndAccountId(TransactionId transactionId, AccountId accountId) =>
        MergeWithTracked(
            databaseContext.AccountBalanceHistories.Where(history => history.TransactionId == transactionId && history.Account.Id == accountId),
            history => history.TransactionId == transactionId && history.Account.Id == accountId)
            .OrderBy(history => history.Date)
            .ToList();

    /// <inheritdoc/>
    public AccountBalanceHistory? GetLatestHistoryEarlierThan(AccountId accountId, DateOnly historyDate, int sequenceNumber) =>
        MergeWithTracked(
            databaseContext.AccountBalanceHistories.Where(history => history.Account.Id == accountId &&
                (history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequenceNumber))),
            history => history.Account.Id == accountId &&
                (history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequenceNumber)))
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountBalanceHistory> GetAllHistoriesLaterThan(AccountId accountId, DateOnly historyDate, int sequence) =>
        MergeWithTracked(
            databaseContext.AccountBalanceHistories.Where(history => history.Account.Id == accountId &&
                (history.Date > historyDate || (history.Date == historyDate && history.Sequence > sequence))),
            history => history.Account.Id == accountId &&
                (history.Date > historyDate || (history.Date == historyDate && history.Sequence > sequence)))
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public void Add(AccountBalanceHistory accountBalanceHistory) => databaseContext.Add(accountBalanceHistory);

    /// <inheritdoc/>
    public void Delete(AccountBalanceHistory accountBalanceHistory) => databaseContext.Remove(accountBalanceHistory);

    /// <summary>
    /// Merges database results with non-deleted tracked histories so a transaction update observes its in-unit-of-work changes.
    /// </summary>
    private IEnumerable<AccountBalanceHistory> MergeWithTracked(
        IQueryable<AccountBalanceHistory> databaseQuery,
        Func<AccountBalanceHistory, bool> predicate)
    {
        var entries = databaseContext.ChangeTracker.Entries<AccountBalanceHistory>().ToList();
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
