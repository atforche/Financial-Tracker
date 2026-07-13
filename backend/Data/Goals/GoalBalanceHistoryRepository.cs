using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Goals;
using Domain.Transactions;

namespace Data.Goals;

/// <summary>
/// Repository that persists Goal Balance Histories.
/// </summary>
public class GoalBalanceHistoryRepository(DatabaseContext databaseContext) : IGoalBalanceHistoryRepository
{
    /// <inheritdoc/>
    public GoalBalanceHistory? GetLatestForFundAndAccountingPeriod(FundId fundId, AccountingPeriodId accountingPeriodId) =>
        Query(fundId, accountingPeriodId)
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<GoalBalanceHistory> GetAllByTransactionId(TransactionId transactionId) =>
        databaseContext.GoalBalanceHistories
            .Where(history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public GoalBalanceHistory? GetLatestHistoryEarlierThan(FundId fundId, AccountingPeriodId accountingPeriodId, DateOnly historyDate, int sequence) =>
        Query(fundId, accountingPeriodId)
            .Where(history => history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequence))
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<GoalBalanceHistory> GetAllHistoriesLaterThan(FundId fundId, AccountingPeriodId accountingPeriodId, DateOnly historyDate, int sequence) =>
        Query(fundId, accountingPeriodId)
            .Where(history => history.Date > historyDate || (history.Date == historyDate && history.Sequence > sequence))
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public void Add(GoalBalanceHistory goalBalanceHistory) => databaseContext.Add(goalBalanceHistory);

    /// <inheritdoc/>
    public void Delete(GoalBalanceHistory goalBalanceHistory) => databaseContext.Remove(goalBalanceHistory);

    private IQueryable<GoalBalanceHistory> Query(FundId fundId, AccountingPeriodId accountingPeriodId) =>
        databaseContext.GoalBalanceHistories.Where(history => history.FundId == fundId && history.AccountingPeriodId == accountingPeriodId);
}