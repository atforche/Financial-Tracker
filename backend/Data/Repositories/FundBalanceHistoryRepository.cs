using Domain.Funds;
using Domain.Transactions;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Fund Balance Histories to be persisted to the database
/// </summary>
public class FundBalanceHistoryRepository(DatabaseContext databaseContext) : IFundBalanceHistoryRepository
{
    /// <inheritdoc/>
    public int GetNextSequenceForFundAndDate(FundId fundId, DateOnly historyDate)
    {
        var historiesOnDate = databaseContext.FundBalanceHistories
            .Where(history => history.FundId == fundId && history.Date == historyDate)
            .ToList();
        return historiesOnDate.Count == 0 ? 1 : historiesOnDate.Max(history => history.Sequence) + 1;
    }

    /// <inheritdoc/>
    public FundBalanceHistory? GetLatestForFund(FundId fundId) =>
        databaseContext.FundBalanceHistories
            .Where(history => history.FundId == fundId)
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundBalanceHistory> GetAllByTransactionId(TransactionId transactionId) =>
        databaseContext.FundBalanceHistories
            .Where(history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ToList();

    /// <inheritdoc/>
    public FundBalanceHistory GetEarliestByTransactionId(FundId fundId, TransactionId transactionId) =>
        databaseContext.FundBalanceHistories
            .Where(history => history.FundId == fundId && history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .FirstOrDefault()
        ?? databaseContext.FundBalanceHistories.Local
            .Where(history => history.FundId == fundId && history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .First();

    /// <inheritdoc/>
    public FundBalanceHistory? GetLatestHistoryEarlierThan(FundId fundId, DateOnly historyDate, int sequenceNumber) =>
        databaseContext.FundBalanceHistories
            .Where(history => history.FundId == fundId &&
                              (history.Date < historyDate ||
                               (history.Date == historyDate && history.Sequence < sequenceNumber)))
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<(FundBalanceHistory History, Transaction Transaction)> GetAllHistoriesLaterThan(FundId fundId, DateOnly historyDate, int sequence)
    {
        var results = databaseContext.FundBalanceHistories
            .Where(history => history.FundId == fundId &&
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
    public void Add(FundBalanceHistory fundBalanceHistory) => databaseContext.Add(fundBalanceHistory);

    /// <inheritdoc/>
    public void Delete(FundBalanceHistory fundBalanceHistory) => databaseContext.Remove(fundBalanceHistory);
}