using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundGoals;

/// <summary>
/// Derives and persists current unposted Fund Goal totals effects.
/// </summary>
public sealed class PendingFundGoalTotalsService(IFundGoalPendingTotalsEffectRepository repository)
{
    /// <summary>
    /// Applies pending effects to one posted Fund Goal totals value.
    /// </summary>
    public FundGoalTotals ApplyPendingEffects(FundGoalTotals postedTotals, AccountingPeriodId accountingPeriodId) =>
        ApplyPendingEffects([(postedTotals, accountingPeriodId)]).Single();

    /// <summary>
    /// Applies pending effects to posted Fund Goal totals values.
    /// </summary>
    public IReadOnlyCollection<FundGoalTotals> ApplyPendingEffects(IReadOnlyCollection<(FundGoalTotals Totals, AccountingPeriodId AccountingPeriodId)> postedTotals)
    {
        var totals = postedTotals.ToDictionary(item => (item.Totals.FundId, item.AccountingPeriodId), item => item.Totals);
        foreach (PendingFundGoalTotalsEffect effect in repository.GetAllByFundAndAccountingPeriodIds(totals.Keys.ToList()))
        {
            FundGoalTotals current = totals[(effect.FundId, effect.AccountingPeriodId)];
            totals[(effect.FundId, effect.AccountingPeriodId)] = new FundGoalTotals(
                current.FundId,
                current.AmountAssigned,
                current.AmountSpent,
                current.AmountAssignedIncludingPending + effect.PendingAmountAssigned,
                current.AmountSpentIncludingPending + effect.PendingAmountSpent);
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
            FundGoalTotals effect = transaction.ApplyAsPostedToFundGoalTotals(new FundGoalTotals(fundId, 0, 0));
            if (effect.AmountAssigned != 0 || effect.AmountSpent != 0)
            {
                repository.Add(new PendingFundGoalTotalsEffect(
                    fundId,
                    transaction.AccountingPeriodId,
                    transaction.Id,
                    effect.AmountAssigned,
                    effect.AmountSpent));
            }
        }
    }

    /// <summary>
    /// Deletes all pending effects for a Transaction.
    /// </summary>
    internal void DeleteEffectsForTransaction(TransactionId transactionId)
    {
        foreach (PendingFundGoalTotalsEffect effect in repository.GetAllByTransactionId(transactionId))
        {
            repository.Delete(effect);
        }
    }
}