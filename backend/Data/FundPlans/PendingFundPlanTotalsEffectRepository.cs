using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Transactions;

namespace Data.FundPlans;

/// <summary>
/// Repository for pending Fund Plan totals effects.
/// </summary>
public sealed class PendingFundPlanTotalsEffectRepository(DatabaseContext databaseContext) : IFundPlanPendingTotalsEffectRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<PendingFundPlanTotalsEffect> GetAllByFundAndAccountingPeriodIds(IReadOnlyCollection<(FundId FundId, AccountingPeriodId AccountingPeriodId)> keys) =>
        keys.Count == 0 ? [] : databaseContext.PendingFundPlanTotalsEffects
            .Where(effect => keys.Select(key => key.FundId).Contains(effect.FundId))
            .Where(effect => keys.Select(key => key.AccountingPeriodId).Contains(effect.AccountingPeriodId))
            .AsEnumerable()
            .Where(effect => keys.Contains((effect.FundId, effect.AccountingPeriodId)))
            .ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<PendingFundPlanTotalsEffect> GetAllByTransactionId(TransactionId transactionId) =>
        databaseContext.PendingFundPlanTotalsEffects.Where(effect => effect.TransactionId == transactionId).ToList();

    /// <inheritdoc/>
    public void Add(PendingFundPlanTotalsEffect effect) => databaseContext.Add(effect);

    /// <inheritdoc/>
    public void Delete(PendingFundPlanTotalsEffect effect) => databaseContext.Remove(effect);
}