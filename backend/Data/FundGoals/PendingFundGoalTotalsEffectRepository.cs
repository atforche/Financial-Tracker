using Domain.AccountingPeriods;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Transactions;

namespace Data.FundGoals;

/// <summary>
/// Repository for pending Fund Goal totals effects.
/// </summary>
public sealed class PendingFundGoalTotalsEffectRepository(DatabaseContext databaseContext) : IFundGoalPendingTotalsEffectRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<PendingFundGoalTotalsEffect> GetAllByFundAndAccountingPeriodIds(IReadOnlyCollection<(FundId FundId, AccountingPeriodId AccountingPeriodId)> keys) =>
        keys.Count == 0 ? [] : databaseContext.PendingFundGoalTotalsEffects
            .Where(effect => keys.Select(key => key.FundId).Contains(effect.FundId))
            .Where(effect => keys.Select(key => key.AccountingPeriodId).Contains(effect.AccountingPeriodId))
            .AsEnumerable()
            .Where(effect => keys.Contains((effect.FundId, effect.AccountingPeriodId)))
            .ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<PendingFundGoalTotalsEffect> GetAllByTransactionId(TransactionId transactionId) =>
        databaseContext.PendingFundGoalTotalsEffects.Where(effect => effect.TransactionId == transactionId).ToList();

    /// <inheritdoc/>
    public void Add(PendingFundGoalTotalsEffect effect) => databaseContext.Add(effect);

    /// <inheritdoc/>
    public void Delete(PendingFundGoalTotalsEffect effect) => databaseContext.Remove(effect);
}
