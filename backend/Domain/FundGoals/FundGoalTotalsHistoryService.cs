using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundGoals;

/// <summary>
/// Maintains posted transaction-level Fund Goal totals history.
/// </summary>
public sealed class FundGoalTotalsHistoryService(IFundGoalTotalsHistoryRepository repository)
{
    /// <summary>
    /// Rebuilds posted histories for a newly added Transaction.
    /// </summary>
    internal void AddTransaction(Transaction transaction) => SynchronizePostedHistories(transaction);

    /// <summary>
    /// Rebuilds posted histories after a Transaction posts to an Account.
    /// </summary>
    internal void PostTransaction(Transaction transaction) => SynchronizePostedHistories(transaction);

    /// <summary>
    /// Removes posted histories before a Transaction's posted dates are cleared.
    /// </summary>
    internal void UnpostTransaction(Transaction transaction) => DeleteTransaction(transaction);

    /// <summary>
    /// Deletes history for a Transaction and reverses its posted effects from later entries.
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (FundGoalTotalsHistory history in repository.GetAllByTransaction(transaction.Id).ToList())
        {
            foreach (FundGoalTotalsHistory later in repository.GetAllLaterThan(
                history.FundId, history.AccountingPeriodId, history.Date, history.Sequence))
            {
                later.Update(transaction.ApplyAllPostedEffectsToFundGoalTotals(later.ToTotals(), reverse: true));
            }
            repository.Delete(history);
        }
    }

    /// <summary>
    /// Replaces a Transaction's histories with its current posted Fund Goal effects.
    /// </summary>
    private void SynchronizePostedHistories(Transaction transaction)
    {
        DeleteTransaction(transaction);
        foreach (FundId fundId in transaction.GetAllAffectedFundIds(null).Where(id => id != Fund.UnassignedFundId).Distinct())
        {
            if (!HasPostedEffect(transaction, fundId))
            {
                continue;
            }
            FundGoalTotals previous = GetPrevious(fundId, transaction);
            var history = new FundGoalTotalsHistory(
                fundId,
                transaction.AccountingPeriodId,
                transaction,
                transaction.ApplyAllPostedEffectsToFundGoalTotals(previous));
            foreach (FundGoalTotalsHistory later in repository.GetAllLaterThan(
                fundId, transaction.AccountingPeriodId, transaction.Date, transaction.Sequence))
            {
                later.Update(transaction.ApplyAllPostedEffectsToFundGoalTotals(later.ToTotals()));
            }
            repository.Add(history);
        }
    }

    /// <summary>
    /// Determines whether the Transaction currently has a posted effect for the Fund.
    /// </summary>
    private static bool HasPostedEffect(Transaction transaction, FundId fundId)
    {
        var accountIds = transaction.GetAllAffectedAccountIds().ToList();
        return accountIds.Count == 0 || accountIds.Any(accountId =>
            transaction.GetPostedDateForAccount(accountId) != null
            && transaction.GetAllAffectedFundIds(accountId).Contains(fundId));
    }

    /// <summary>
    /// Retrieves the most recent Fund Goal totals for a given fund prior to a specific transaction, or returns default totals if none exist.
    /// </summary>
    private FundGoalTotals GetPrevious(FundId fundId, Transaction transaction) =>
        repository.GetLatestEarlierThan(fundId, transaction.AccountingPeriodId, transaction.Date, transaction.Sequence)?.ToTotals()
        ?? new FundGoalTotals(fundId, 0, 0);
}