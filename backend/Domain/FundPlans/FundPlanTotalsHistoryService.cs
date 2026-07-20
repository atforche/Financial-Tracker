using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundPlans;

/// <summary>
/// Maintains transaction-level Fund Plan totals history.
/// </summary>
public sealed class FundPlanTotalsHistoryService(IFundPlanTotalsHistoryRepository repository)
{
    /// <summary>
    /// Adds history for a new Transaction.
    /// </summary>
    internal void AddTransaction(Transaction transaction)
    {
        foreach (FundId fundId in transaction.GetAllAffectedFundIds(null).Where(id => id != Fund.UnassignedFundId))
        {
            FundPlanTotals previous = GetPrevious(fundId, transaction);
            var history = new FundPlanTotalsHistory(
                fundId,
                transaction.AccountingPeriodId,
                transaction,
                transaction.ApplyToFundPlanTotals(previous));
            foreach (FundPlanTotalsHistory later in repository.GetAllLaterThan(
                fundId, transaction.AccountingPeriodId, transaction.Date, transaction.Sequence))
            {
                later.Update(transaction.ApplyToFundPlanTotals(later.ToTotals()));
            }
            repository.Add(history);
        }
    }

    /// <summary>
    /// Applies posting changes to existing history.
    /// </summary>
    internal void PostTransaction(Transaction transaction, AccountId accountId) =>
        ApplyPosting(transaction, accountId, reverse: false);

    /// <summary>
    /// Reverses posting changes from existing history.
    /// </summary>
    internal void UnpostTransaction(Transaction transaction)
    {
        foreach (AccountId accountId in transaction.GetAllAffectedAccountIds()
            .Where(id => transaction.GetPostedDateForAccount(id) != null))
        {
            ApplyPosting(transaction, accountId, reverse: true);
        }
    }

    /// <summary>
    /// Deletes history for a Transaction and reverses its effect from later entries.
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (FundPlanTotalsHistory history in repository.GetAllByTransaction(transaction.Id))
        {
            foreach (FundPlanTotalsHistory later in repository.GetAllLaterThan(
                history.FundId, history.AccountingPeriodId, history.Date, history.Sequence))
            {
                later.Update(transaction.ApplyToFundPlanTotals(later.ToTotals(), reverse: true));
            }
            repository.Delete(history);
        }
    }

    /// <summary>
    /// Applies posting changes to existing history for a specific account and reverses them if needed.
    /// </summary>
    private void ApplyPosting(Transaction transaction, AccountId accountId, bool reverse)
    {
        foreach (FundPlanTotalsHistory history in repository.GetAllByTransaction(transaction.Id)
            .Where(item => transaction.GetAllAffectedFundIds(accountId).Contains(item.FundId)))
        {
            history.Update(transaction.ApplyToFundPlanTotals(
                history.ToTotals(), accountId: accountId, reverse: reverse, postingOnly: true));
            foreach (FundPlanTotalsHistory later in repository.GetAllLaterThan(
                history.FundId, history.AccountingPeriodId, history.Date, history.Sequence))
            {
                later.Update(transaction.ApplyToFundPlanTotals(
                    later.ToTotals(), accountId: accountId, reverse: reverse, postingOnly: true));
            }
        }
    }

    /// <summary>
    /// Retrieves the most recent Fund Plan totals for a given fund prior to a specific transaction, or returns default totals if none exist.
    /// </summary>
    private FundPlanTotals GetPrevious(FundId fundId, Transaction transaction) =>
        repository.GetLatestEarlierThan(
            fundId, transaction.AccountingPeriodId, transaction.Date, transaction.Sequence)?.ToTotals()
        ?? new FundPlanTotals(fundId, 0, 0, 0, 0);
}