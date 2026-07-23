using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundPlans;

/// <summary>
/// Derives and persists current unposted Fund Plan totals effects.
/// </summary>
public sealed class PendingFundPlanTotalsService(IFundPlanPendingTotalsEffectRepository repository)
{
    /// <summary>
    /// Applies pending effects to one posted Fund Plan totals value.
    /// </summary>
    public FundPlanTotals ApplyPendingEffects(FundPlanTotals postedTotals, AccountingPeriodId accountingPeriodId) =>
        ApplyPendingEffects([(postedTotals, accountingPeriodId)]).Single();

    /// <summary>
    /// Applies pending effects to posted Fund Plan totals values.
    /// </summary>
    public IReadOnlyCollection<FundPlanTotals> ApplyPendingEffects(IReadOnlyCollection<(FundPlanTotals Totals, AccountingPeriodId AccountingPeriodId)> postedTotals)
    {
        var totals = postedTotals.ToDictionary(item => (item.Totals.FundId, item.AccountingPeriodId), item => item.Totals);
        foreach (PendingFundPlanTotalsEffect effect in repository.GetAllByFundAndAccountingPeriodIds(totals.Keys.ToList()))
        {
            FundPlanTotals current = totals[(effect.FundId, effect.AccountingPeriodId)];
            totals[(effect.FundId, effect.AccountingPeriodId)] = current
                .AddNewPendingAmountAssigned(effect.PendingAmountAssigned)
                .AddNewPendingAmountSpent(effect.PendingAmountSpent);
        }
        return postedTotals.Select(item => totals[(item.Totals.FundId, item.AccountingPeriodId)]).ToList();
    }

    /// <summary>
    /// Rebuilds pending effects for a Transaction, replacing any existing effects for that Transaction.
    /// </summary>
    internal void SynchronizeTransaction(Transaction transaction)
    {
        DeleteEffectsForTransaction(transaction.Id);
        foreach (FundId fundId in transaction.GetAllAffectedFundIds(null).Where(id => id != Fund.UnassignedFundId).Distinct())
        {
            FundPlanTotals effect = transaction.ApplyToFundPlanTotals(new FundPlanTotals(fundId, 0, 0, 0, 0));
            if (effect.PendingAmountAssigned != 0 || effect.PendingAmountSpent != 0)
            {
                repository.Add(new PendingFundPlanTotalsEffect(
                    fundId,
                    transaction.AccountingPeriodId,
                    transaction.Id,
                    effect.PendingAmountAssigned,
                    effect.PendingAmountSpent));
            }
        }
    }

    /// <summary>
    /// Deletes all pending effects for a Transaction.
    /// </summary>
    internal void DeleteEffectsForTransaction(TransactionId transactionId)
    {
        foreach (PendingFundPlanTotalsEffect effect in repository.GetAllByTransactionId(transactionId))
        {
            repository.Delete(effect);
        }
    }
}