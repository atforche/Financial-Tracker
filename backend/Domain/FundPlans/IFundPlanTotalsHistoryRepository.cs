using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundPlans;

/// <summary>
/// Persistence contract for Fund Plan totals history.
/// </summary>
public interface IFundPlanTotalsHistoryRepository
{
    /// <summary>
    /// Gets history entries created by a Transaction.
    /// </summary>
    IReadOnlyCollection<FundPlanTotalsHistory> GetAllByTransaction(TransactionId transactionId);

    /// <summary>
    /// Gets the latest entry before the provided ordering key.
    /// </summary>
    FundPlanTotalsHistory? GetLatestEarlierThan(FundId fundId, AccountingPeriodId accountingPeriodId, DateOnly historyDate, int sequence);

    /// <summary>
    /// Gets entries after the provided ordering key.
    /// </summary>
    IReadOnlyCollection<FundPlanTotalsHistory> GetAllLaterThan(FundId fundId, AccountingPeriodId accountingPeriodId, DateOnly historyDate, int sequence);

    /// <summary>
    /// Adds a history entry.
    /// </summary>
    void Add(FundPlanTotalsHistory history);

    /// <summary>
    /// Deletes a history entry.
    /// </summary>
    void Delete(FundPlanTotalsHistory history);
}