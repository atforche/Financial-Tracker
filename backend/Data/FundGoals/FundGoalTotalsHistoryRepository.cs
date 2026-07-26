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
        databaseContext.FundGoalTotalsHistories
            .Where(history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public FundGoalTotalsHistory? GetLatestEarlierThan(
        FundId fundId,
        AccountingPeriodId accountingPeriodId,
        DateOnly historyDate,
        int sequence) => Query(fundId, accountingPeriodId)
            .Where(history => history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequence))
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundGoalTotalsHistory> GetAllLaterThan(
        FundId fundId,
        AccountingPeriodId accountingPeriodId,
        DateOnly historyDate,
        int sequence) => Query(fundId, accountingPeriodId)
            .Where(history => history.Date > historyDate || (history.Date == historyDate && history.Sequence > sequence))
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
}