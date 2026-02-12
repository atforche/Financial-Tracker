using Domain.Accounts;
using Domain.Transactions;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Account Balance Histories to be persisted to the database
/// </summary>
public class AccountBalanceHistoryRepository(DatabaseContext databaseContext) : IAccountBalanceHistoryRepository
{
    /// <inheritdoc/>
    public int GetNextSequenceForAccountAndDate(AccountId accountId, DateOnly historyDate)
    {
        var historiesOnDate = databaseContext.AccountBalanceHistories
            .Where(history => history.AccountId == accountId && history.Date == historyDate)
            .ToList();
        return historiesOnDate.Count == 0 ? 1 : historiesOnDate.Max(history => history.Sequence) + 1;
    }

    /// <inheritdoc/>
    public AccountBalanceHistory GetLatestForAccount(AccountId accountId) =>
        databaseContext.AccountBalanceHistories
            .Where(history => history.AccountId == accountId)
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .First();

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountBalanceHistory> GetAllByTransactionId(TransactionId transactionId) =>
        databaseContext.AccountBalanceHistories
            .Where(history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ToList();

    /// <inheritdoc/>
    public AccountBalanceHistory GetEarliestByTransactionId(AccountId accountId, TransactionId transactionId) =>
        databaseContext.AccountBalanceHistories
            .Where(history => history.AccountId == accountId && history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .FirstOrDefault()
        ?? databaseContext.AccountBalanceHistories.Local
            .Where(history => history.AccountId == accountId && history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .First();

    /// <inheritdoc/>
    public AccountBalanceHistory? GetLatestHistoryEarlierThan(AccountId accountId, DateOnly historyDate, int sequenceNumber) =>
        databaseContext.AccountBalanceHistories
            .Where(history => history.AccountId == accountId &&
                              (history.Date < historyDate ||
                               (history.Date == historyDate && history.Sequence < sequenceNumber)))
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<(AccountBalanceHistory History, Transaction Transaction)> GetAllHistoriesLaterThan(AccountId accountId, DateOnly historyDate, int sequence)
    {
        var results = databaseContext.AccountBalanceHistories
            .Where(history => history.AccountId == accountId &&
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
    public void Add(AccountBalanceHistory accountBalanceHistory) => databaseContext.Add(accountBalanceHistory);

    /// <inheritdoc/>
    public void Delete(AccountBalanceHistory accountBalanceHistory) => databaseContext.Remove(accountBalanceHistory);
}