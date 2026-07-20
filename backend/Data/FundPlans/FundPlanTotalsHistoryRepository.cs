using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Transactions;

namespace Data.FundPlans;

/// <summary>
/// Repository that persists Fund Plan totals history.
/// </summary>
public sealed class FundPlanTotalsHistoryRepository(DatabaseContext databaseContext) : IFundPlanTotalsHistoryRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<FundPlanTotalsHistory> GetAllByTransaction(TransactionId transactionId) =>
        databaseContext.FundPlanTotalsHistories
            .Where(history => history.TransactionId == transactionId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public FundPlanTotalsHistory? GetLatestEarlierThan(
        FundId fundId,
        AccountingPeriodId accountingPeriodId,
        DateOnly historyDate,
        int sequence) => Query(fundId, accountingPeriodId)
            .Where(history => history.Date < historyDate || (history.Date == historyDate && history.Sequence < sequence))
            .OrderByDescending(history => history.Date)
            .ThenByDescending(history => history.Sequence)
            .FirstOrDefault();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundPlanTotalsHistory> GetAllLaterThan(
        FundId fundId,
        AccountingPeriodId accountingPeriodId,
        DateOnly historyDate,
        int sequence) => Query(fundId, accountingPeriodId)
            .Where(history => history.Date > historyDate || (history.Date == historyDate && history.Sequence > sequence))
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .ToList();

    /// <inheritdoc/>
    public void Add(FundPlanTotalsHistory history) => databaseContext.Add(history);

    /// <inheritdoc/>
    public void Delete(FundPlanTotalsHistory history) => databaseContext.Remove(history);

    private IQueryable<FundPlanTotalsHistory> Query(FundId fundId, AccountingPeriodId accountingPeriodId) =>
        databaseContext.FundPlanTotalsHistories.Where(history =>
            history.FundId == fundId && history.AccountingPeriodId == accountingPeriodId);
}