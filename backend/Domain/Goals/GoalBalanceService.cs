using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.Goals;

/// <summary>
/// Service for managing Goal Balances.
/// </summary>
public class GoalBalanceService(
    IGoalBalanceHistoryRepository goalBalanceHistoryRepository,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Gets the current balance for the provided Fund and Accounting Period.
    /// </summary>
    public GoalBalance GetCurrentBalance(FundId fundId, AccountingPeriodId accountingPeriodId) =>
        goalBalanceHistoryRepository.GetLatestForFundAndAccountingPeriod(fundId, accountingPeriodId)?.ToGoalBalance()
        ?? new GoalBalance(fundId, 0, 0, 0, 0);

    /// <summary>
    /// Updates Goal Balance Histories for a newly added Transaction.
    /// </summary>
    internal void AddTransaction(Transaction transaction)
    {
        foreach (FundId fundId in GetGoalAffectedFundIds(transaction, null))
        {
            AddNewBalanceHistory(transaction, fundId, transaction.Date);
        }
    }

    /// <summary>
    /// Updates Goal Balance Histories for a newly posted Transaction.
    /// </summary>
    internal void PostTransaction(Transaction transaction, AccountId accountId)
    {
        DateOnly? postedDate = transaction.GetPostedDateForAccount(accountId);
        if (postedDate == null)
        {
            return;
        }
        foreach (FundId fundId in GetGoalAffectedFundIds(transaction, accountId))
        {
            if (postedDate == transaction.Date)
            {
                UpdateExistingBalanceHistory(transaction, fundId, accountId);
            }
            else
            {
                GoalBalanceHistory? postedHistory = goalBalanceHistoryRepository
                    .GetAllByTransactionId(transaction.Id)
                    .SingleOrDefault(history => history.FundId == fundId && history.Date == postedDate.Value);
                if (postedHistory == null)
                {
                    AddNewBalanceHistory(transaction, fundId, postedDate.Value);
                }
                else
                {
                    UpdateExistingBalanceHistory(transaction, fundId, accountId, postedHistory);
                }
            }
        }
    }

    /// <summary>
    /// Updates Goal Balance Histories for a newly unposted Transaction.
    /// </summary>
    internal void UnpostTransaction(Transaction transaction)
    {
        IEnumerable<IGrouping<(FundId FundId, DateOnly PostedDate), (FundId FundId, AccountId AccountId, DateOnly PostedDate)>> postedFundAccounts = transaction.GetAllAffectedAccountIds()
            .SelectMany(accountId =>
            {
                DateOnly? postedDate = transaction.GetPostedDateForAccount(accountId);
                return postedDate == null
                    ? []
                    : GetGoalAffectedFundIds(transaction, accountId)
                        .Select(fundId => (FundId: fundId, AccountId: accountId, PostedDate: postedDate.Value));
            })
            .GroupBy(posting => (posting.FundId, posting.PostedDate));

        foreach (IGrouping<(FundId FundId, DateOnly PostedDate), (FundId FundId, AccountId AccountId, DateOnly PostedDate)> postingGroup in postedFundAccounts)
        {
            if (postingGroup.Key.PostedDate == transaction.Date)
            {
                GoalBalanceHistory existingHistory = goalBalanceHistoryRepository
                    .GetAllByTransactionId(transaction.Id)
                    .Single(history => history.FundId == postingGroup.Key.FundId && history.Date == transaction.Date);
                foreach (AccountId accountId in postingGroup.Select(posting => posting.AccountId))
                {
                    existingHistory.Update(transaction.ApplyToGoalBalance(
                        existingHistory.ToGoalBalance(),
                        accountId: accountId,
                        reverse: true,
                        postingOnly: true));
                    UpdateLaterBalanceHistoriesForPosting(transaction, existingHistory, accountId, reverse: true);
                }
                continue;
            }
            GoalBalanceHistory postedHistory = goalBalanceHistoryRepository
                .GetAllByTransactionId(transaction.Id)
                .Single(history => history.FundId == postingGroup.Key.FundId && history.Date == postingGroup.Key.PostedDate);
            RemovePostedBalanceHistory(transaction, postedHistory, postingGroup.Select(posting => posting.AccountId));
            goalBalanceHistoryRepository.Delete(postedHistory);
        }
    }

    /// <summary>
    /// Removes Goal Balance Histories for a deleted Transaction.
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (GoalBalanceHistory history in goalBalanceHistoryRepository.GetAllByTransactionId(transaction.Id))
        {
            DeleteExistingBalanceHistory(transaction, history);
            goalBalanceHistoryRepository.Delete(history);
        }
    }

    /// <summary>
    /// Adds a Goal Balance History entry and recalculates later histories.
    /// </summary>
    private void AddNewBalanceHistory(Transaction transaction, FundId fundId, DateOnly date)
    {
        int sequence = GetSequenceForTransaction(fundId, transaction, date);
        GoalBalance existingBalance = GetExistingGoalBalanceAsOf(fundId, transaction.AccountingPeriodId, date, sequence);
        var history = new GoalBalanceHistory(
            fundId,
            transaction.AccountingPeriodId,
            transaction.Id,
            date,
            sequence,
            transaction.ApplyToGoalBalance(existingBalance, date));

        foreach (GoalBalanceHistory laterHistory in goalBalanceHistoryRepository.GetAllHistoriesLaterThan(
            fundId,
            transaction.AccountingPeriodId,
            date,
            sequence))
        {
            if (laterHistory.Date == date)
            {
                laterHistory.Sequence += 1;
            }
            GoalBalance updatedBalance = transaction.ApplyToGoalBalance(laterHistory.ToGoalBalance(), date);
            laterHistory.Update(updatedBalance);
        }
        goalBalanceHistoryRepository.Add(history);
    }

    /// <summary>
    /// Recalculates a Goal Balance History and applies a posted-account delta to later histories.
    /// </summary>
    private void UpdateExistingBalanceHistory(
        Transaction transaction,
        FundId fundId,
        AccountId accountId,
        GoalBalanceHistory? historyToUpdate = null)
    {
        GoalBalanceHistory existingHistory = historyToUpdate ?? goalBalanceHistoryRepository.GetAllByTransactionId(transaction.Id)
            .Where(history => history.FundId == fundId)
            .OrderBy(history => history.Date)
            .ThenBy(history => history.Sequence)
            .First();
        GoalBalance existingBalance = GetExistingGoalBalanceAsOf(
            fundId,
            transaction.AccountingPeriodId,
            existingHistory.Date,
            existingHistory.Sequence);
        existingHistory.Update(transaction.ApplyToGoalBalance(existingBalance, existingHistory.Date));

        UpdateLaterBalanceHistoriesForPosting(transaction, existingHistory, accountId);
    }

    /// <summary>
    /// Applies a posted-account delta to Goal Balance History entries after the provided entry.
    /// </summary>
    private void UpdateLaterBalanceHistoriesForPosting(
        Transaction transaction,
        GoalBalanceHistory balanceHistory,
        AccountId accountId,
        bool reverse = false)
    {
        foreach (GoalBalanceHistory history in goalBalanceHistoryRepository.GetAllHistoriesLaterThan(
            balanceHistory.FundId,
            transaction.AccountingPeriodId,
            balanceHistory.Date,
            balanceHistory.Sequence))
        {
            GoalBalance updatedBalance = transaction.ApplyToGoalBalance(
                history.ToGoalBalance(),
                accountId: accountId,
                reverse: reverse,
                postingOnly: true);
            history.Update(updatedBalance);
        }
    }

    /// <summary>
    /// Removes a posted Goal Balance History entry and reverses its posting effects from later entries.
    /// </summary>
    private void RemovePostedBalanceHistory(
        Transaction transaction,
        GoalBalanceHistory deletedBalanceHistory,
        IEnumerable<AccountId> accountIds)
    {
        foreach (GoalBalanceHistory history in goalBalanceHistoryRepository.GetAllHistoriesLaterThan(
            deletedBalanceHistory.FundId,
            deletedBalanceHistory.AccountingPeriodId,
            deletedBalanceHistory.Date,
            deletedBalanceHistory.Sequence + 1))
        {
            if (history.Date == deletedBalanceHistory.Date)
            {
                history.Sequence -= 1;
            }
            var updatedBalance = history.ToGoalBalance();
            foreach (AccountId accountId in accountIds)
            {
                updatedBalance = transaction.ApplyToGoalBalance(
                    updatedBalance,
                    accountId: accountId,
                    reverse: true,
                    postingOnly: true);
            }
            history.Update(updatedBalance);
        }
    }

    /// <summary>
    /// Removes a Goal Balance History entry's effect from all later histories.
    /// </summary>
    private void DeleteExistingBalanceHistory(Transaction transaction, GoalBalanceHistory deletedHistory)
    {
        foreach (GoalBalanceHistory laterHistory in goalBalanceHistoryRepository.GetAllHistoriesLaterThan(
            deletedHistory.FundId,
            deletedHistory.AccountingPeriodId,
            deletedHistory.Date,
            deletedHistory.Sequence + 1))
        {
            if (laterHistory.Date == deletedHistory.Date)
            {
                laterHistory.Sequence -= 1;
            }
            GoalBalance updatedBalance = transaction.ApplyToGoalBalance(
                laterHistory.ToGoalBalance(),
                deletedHistory.Date,
                reverse: true);
            laterHistory.Update(updatedBalance);
        }
    }

    /// <summary>
    /// Gets the history sequence for a transaction on a date.
    /// </summary>
    private int GetSequenceForTransaction(FundId fundId, Transaction transaction, DateOnly date) =>
        goalBalanceHistoryRepository.GetAllHistoriesLaterThan(fundId, transaction.AccountingPeriodId, date, 0)
            .Count(history => history.Date == date && transactionRepository.GetById(history.TransactionId).Sequence < transaction.Sequence) + 1;

    /// <summary>
    /// Gets the transaction's affected Funds that can participate in goal progress.
    /// </summary>
    private static IEnumerable<FundId> GetGoalAffectedFundIds(Transaction transaction, AccountId? accountId) =>
        transaction.GetAllAffectedFundIds(accountId)
            .Where(fundId => fundId != Fund.UnassignedFundId);

    /// <summary>
    /// Gets the Goal Balance immediately before a date and sequence.
    /// </summary>
    private GoalBalance GetExistingGoalBalanceAsOf(
        FundId fundId,
        AccountingPeriodId accountingPeriodId,
        DateOnly date,
        int sequence) =>
        goalBalanceHistoryRepository.GetLatestHistoryEarlierThan(fundId, accountingPeriodId, date, sequence)?.ToGoalBalance()
        ?? new GoalBalance(fundId, 0, 0, 0, 0);
}