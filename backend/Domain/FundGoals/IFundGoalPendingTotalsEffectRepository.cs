using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundGoals;

/// <summary>
/// Persists pending Fund Goal totals effects.
/// </summary>
public interface IFundGoalPendingTotalsEffectRepository
{
    /// <summary>
    /// Gets effects for the provided Fund and Accounting Period pairs.
    /// </summary>
    IReadOnlyCollection<PendingFundGoalTotalsEffect> GetAllByFundAndAccountingPeriodIds(IReadOnlyCollection<(FundId FundId, AccountingPeriodId AccountingPeriodId)> keys);

    /// <summary>
    /// Gets effects created by a Transaction.
    /// </summary>
    IReadOnlyCollection<PendingFundGoalTotalsEffect> GetAllByTransactionId(TransactionId transactionId);

    /// <summary>
    /// Adds a pending effect.
    /// </summary>
    void Add(PendingFundGoalTotalsEffect effect);

    /// <summary>
    /// Deletes a pending effect.
    /// </summary>
    void Delete(PendingFundGoalTotalsEffect effect);
}