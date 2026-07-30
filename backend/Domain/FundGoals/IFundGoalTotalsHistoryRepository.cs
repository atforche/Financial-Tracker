using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundGoals;

/// <summary>
/// Persistence contract for Fund Goal totals history.
/// </summary>
public interface IFundGoalTotalsHistoryRepository
{
    /// <summary>
    /// Gets history entries created by a Transaction.
    /// </summary>
    IReadOnlyCollection<FundGoalTotalsHistory> GetAllByTransaction(TransactionId transactionId);

    /// <summary>
    /// Gets the latest entry before the provided ordering key.
    /// </summary>
    FundGoalTotalsHistory? GetLatestEarlierThan(FundId fundId, AccountingPeriodId accountingPeriodId, DateOnly historyDate, int sequence);

    /// <summary>
    /// Gets entries after the provided ordering key.
    /// </summary>
    IReadOnlyCollection<FundGoalTotalsHistory> GetAllLaterThan(FundId fundId, AccountingPeriodId accountingPeriodId, DateOnly historyDate, int sequence);

    /// <summary>
    /// Adds a history entry.
    /// </summary>
    void Add(FundGoalTotalsHistory history);

    /// <summary>
    /// Deletes a history entry.
    /// </summary>
    void Delete(FundGoalTotalsHistory history);
}