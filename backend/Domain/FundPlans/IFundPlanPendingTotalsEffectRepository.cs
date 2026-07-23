using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundPlans;

/// <summary>
/// Persists pending Fund Plan totals effects.
/// </summary>
public interface IFundPlanPendingTotalsEffectRepository
{
    /// <summary>
    /// Gets effects for the provided Fund and Accounting Period pairs.
    /// </summary>
    IReadOnlyCollection<PendingFundPlanTotalsEffect> GetAllByFundAndAccountingPeriodIds(IReadOnlyCollection<(FundId FundId, AccountingPeriodId AccountingPeriodId)> keys);

    /// <summary>
    /// Gets effects created by a Transaction.
    /// </summary>
    IReadOnlyCollection<PendingFundPlanTotalsEffect> GetAllByTransactionId(TransactionId transactionId);

    /// <summary>
    /// Adds a pending effect.
    /// </summary>
    void Add(PendingFundPlanTotalsEffect effect);

    /// <summary>
    /// Deletes a pending effect.
    /// </summary>
    void Delete(PendingFundPlanTotalsEffect effect);
}