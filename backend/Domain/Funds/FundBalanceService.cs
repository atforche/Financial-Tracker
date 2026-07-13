using Domain.Accounts;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Fund Balances
/// </summary>
public class FundBalanceService(
    IFundBalanceHistoryRepository fundBalanceHistoryRepository,
    ITransactionRepository transactionRepository,
    IFundRepository fundRepository)
{
    /// <summary>
    /// Gets the current balance for the provided Account
    /// </summary>
    public FundBalance GetCurrentBalance(FundId fundId) =>
        fundBalanceHistoryRepository.GetLatestForFund(fundId)?.ToFundBalance() ??
            new FundBalance(fundId, fundRepository.GetById(fundId).OnboardedBalance ?? 0, 0, 0);

    /// <summary>
    /// Gets the Fund Balances prior to the provided Transaction
    /// </summary>
    public IEnumerable<FundBalance> GetPreviousBalancesForTransaction(Transaction transaction) =>
        transaction.GetAllAffectedFundIds(null)
            .Select(fundId =>
            {
                FundBalanceHistory latestHistory = fundBalanceHistoryRepository.GetAllByTransactionId(transaction.Id)
                    .Where(fundBalanceHistory => fundBalanceHistory.FundId == fundId)
                    .OrderBy(fundBalanceHistory => fundBalanceHistory.Date)
                    .ThenBy(fundBalanceHistory => fundBalanceHistory.Sequence)
                    .First();
                return GetExistingFundBalanceAsOf(fundId, latestHistory.Date, latestHistory.Sequence);
            });

    /// <summary>
    /// Gets the Fund Balances after the provided Transaction
    /// </summary>
    public IEnumerable<FundBalance> GetNewBalanceForTransaction(Transaction transaction) =>
        transaction.GetAllAffectedFundIds(null)
            .Select(fundId =>
            {
                FundBalanceHistory latestHistory = fundBalanceHistoryRepository.GetAllByTransactionId(transaction.Id)
                    .Where(fundBalanceHistory => fundBalanceHistory.FundId == fundId)
                    .OrderByDescending(fundBalanceHistory => fundBalanceHistory.Date)
                    .ThenByDescending(fundBalanceHistory => fundBalanceHistory.Sequence)
                    .First();
                return latestHistory.ToFundBalance();
            });

    /// <summary>
    /// Updates the Fund Balances for a newly added Transaction
    /// </summary>
    internal void AddTransaction(Transaction newTransaction)
    {
        foreach (FundId fund in newTransaction.GetAllAffectedFundIds(null))
        {
            AddNewBalanceHistory(newTransaction, fund, newTransaction.Date);
        }
    }

    /// <summary>
    /// Updates the Account Balances for a newly posted Transaction
    /// </summary>
    internal void PostTransaction(Transaction transaction, AccountId accountId)
    {
        DateOnly? postedDate = transaction.GetPostedDateForAccount(accountId);
        if (postedDate == null)
        {
            return;
        }
        IEnumerable<FundId> affectedFunds = transaction.GetAllAffectedFundIds(accountId);
        foreach (FundId fund in affectedFunds)
        {
            if (postedDate == transaction.Date)
            {
                UpdateExistingBalanceHistory(transaction, fund, accountId);
            }
            else
            {
                FundBalanceHistory? postedHistory = fundBalanceHistoryRepository
                    .GetAllByTransactionId(transaction.Id)
                    .SingleOrDefault(history => history.FundId == fund && history.Date == postedDate.Value);
                if (postedHistory == null)
                {
                    AddNewBalanceHistory(transaction, fund, postedDate.Value);
                }
                else
                {
                    UpdateExistingBalanceHistory(transaction, fund, accountId, postedHistory);
                }
            }
        }
    }

    /// <summary>
    /// Updates the Fund Balances for an unposted Transaction
    /// </summary>
    internal void UnpostTransaction(Transaction transaction)
    {
        IEnumerable<IGrouping<(FundId FundId, DateOnly PostedDate), (FundId FundId, AccountId AccountId, DateOnly PostedDate)>> postedFundAccounts = transaction.GetAllAffectedAccountIds()
            .SelectMany(accountId =>
            {
                DateOnly? postedDate = transaction.GetPostedDateForAccount(accountId);
                return postedDate == null
                    ? []
                    : transaction.GetAllAffectedFundIds(accountId)
                        .Select(fundId => (FundId: fundId, AccountId: accountId, PostedDate: postedDate.Value));
            })
            .GroupBy(posting => (posting.FundId, posting.PostedDate));

        foreach (IGrouping<(FundId FundId, DateOnly PostedDate), (FundId FundId, AccountId AccountId, DateOnly PostedDate)> postingGroup in postedFundAccounts)
        {
            if (postingGroup.Key.PostedDate == transaction.Date)
            {
                FundBalanceHistory existingHistory = fundBalanceHistoryRepository
                    .GetAllByTransactionId(transaction.Id)
                    .Single(history => history.FundId == postingGroup.Key.FundId && history.Date == transaction.Date);
                foreach (AccountId accountId in postingGroup.Select(posting => posting.AccountId))
                {
                    existingHistory.Update(transaction.ApplyToFundBalance(
                        existingHistory.ToFundBalance(),
                        accountId: accountId,
                        reverse: true,
                        postingOnly: true));
                    UpdateLaterBalanceHistoriesForPosting(transaction, existingHistory, accountId, reverse: true);
                }
                continue;
            }
            FundBalanceHistory postedHistory = fundBalanceHistoryRepository
                .GetAllByTransactionId(transaction.Id)
                .Single(history => history.FundId == postingGroup.Key.FundId && history.Date == postingGroup.Key.PostedDate);
            RemovePostedBalanceHistory(transaction, postedHistory, postingGroup.Select(posting => posting.AccountId));
            fundBalanceHistoryRepository.Delete(postedHistory);
        }
    }

    /// <summary>
    /// Updates the Fund Balances for a deleted Transaction
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (FundBalanceHistory balanceHistory in fundBalanceHistoryRepository.GetAllByTransactionId(transaction.Id))
        {
            DeleteExistingBalanceHistory(transaction, balanceHistory);
            fundBalanceHistoryRepository.Delete(balanceHistory);
        }
    }

    /// <summary>
    /// Adds a new Fund Balance History entry
    /// </summary>
    private void AddNewBalanceHistory(Transaction transaction, FundId fund, DateOnly date)
    {
        int sequence = GetSequenceForTransaction(fund, transaction, date);
        FundBalance existingBalance = GetExistingFundBalanceAsOf(fund, date, sequence);
        var newBalanceHistory = new FundBalanceHistory(
            fund,
            transaction.Id,
            date,
            sequence,
            transaction.ApplyToFundBalance(existingBalance, date));

        foreach (FundBalanceHistory history in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(newBalanceHistory.FundId, newBalanceHistory.Date, newBalanceHistory.Sequence))
        {
            if (history.Date == newBalanceHistory.Date)
            {
                history.Sequence += 1;
            }
            existingBalance = history.ToFundBalance();
            FundBalance updatedBalance = transaction.ApplyToFundBalance(existingBalance, date);
            history.Update(updatedBalance);
        }
        fundBalanceHistoryRepository.Add(newBalanceHistory);
    }

    /// <summary>
    /// Updates an existing Fund Balance History entry
    /// </summary>
    private void UpdateExistingBalanceHistory(
        Transaction transaction,
        FundId fund,
        AccountId accountId,
        FundBalanceHistory? historyToUpdate = null)
    {
        FundBalanceHistory existingHistory = historyToUpdate ?? fundBalanceHistoryRepository.GetEarliestByTransactionId(fund, transaction.Id);
        FundBalance existingBalance = GetExistingFundBalanceAsOf(fund, existingHistory.Date, existingHistory.Sequence);
        existingHistory.Update(transaction.ApplyToFundBalance(existingBalance, existingHistory.Date));
        UpdateLaterBalanceHistoriesForPosting(transaction, existingHistory, accountId);
    }

    /// <summary>
    /// Applies a posted-account delta to Fund Balance History entries after the provided entry.
    /// </summary>
    private void UpdateLaterBalanceHistoriesForPosting(
        Transaction transaction,
        FundBalanceHistory balanceHistory,
        AccountId accountId,
        bool reverse = false)
    {
        foreach (FundBalanceHistory history in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(balanceHistory.FundId, balanceHistory.Date, balanceHistory.Sequence))
        {
            FundBalance updatedBalance = transaction.ApplyToFundBalance(
                history.ToFundBalance(),
                accountId: accountId,
                reverse: reverse,
                postingOnly: true);
            history.Update(updatedBalance);
        }
    }

    /// <summary>
    /// Removes a posted Fund Balance History entry and reverses its posting effects from later entries.
    /// </summary>
    private void RemovePostedBalanceHistory(
        Transaction transaction,
        FundBalanceHistory deletedBalanceHistory,
        IEnumerable<AccountId> accountIds)
    {
        foreach (FundBalanceHistory history in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(deletedBalanceHistory.FundId, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence + 1))
        {
            if (history.Date == deletedBalanceHistory.Date)
            {
                history.Sequence -= 1;
            }
            var updatedBalance = history.ToFundBalance();
            foreach (AccountId accountId in accountIds)
            {
                updatedBalance = transaction.ApplyToFundBalance(
                    updatedBalance,
                    accountId: accountId,
                    reverse: true,
                    postingOnly: true);
            }
            history.Update(updatedBalance);
        }
    }

    /// <summary>
    /// Deletes an existing Fund Balance History entry
    /// </summary>
    private void DeleteExistingBalanceHistory(Transaction transaction, FundBalanceHistory deletedBalanceHistory)
    {
        FundBalance existingBalance = GetExistingFundBalanceAsOf(deletedBalanceHistory.FundId, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence);
        foreach (FundBalanceHistory history in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(deletedBalanceHistory.FundId, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence + 1))
        {
            if (history.Date == deletedBalanceHistory.Date)
            {
                history.Sequence -= 1;
            }
            FundBalance updatedBalance = transaction.ApplyToFundBalance(existingBalance, deletedBalanceHistory.Date, reverse: true);
            history.Update(updatedBalance);
            existingBalance = updatedBalance;
        }
    }

    /// <summary>
    /// Gets the balance history sequence number for the provided Transaction.
    /// </summary>
    /// <remarks>
    /// This keeps rebuilt balance history entries in transaction-sequence order when an updated transaction is removed and re-added on the same date.
    /// </remarks>
    private int GetSequenceForTransaction(FundId fundId, Transaction transaction, DateOnly date) =>
        fundBalanceHistoryRepository.GetAllHistoriesLaterThan(fundId, date, 0)
            .Count(history => history.Date == date && transactionRepository.GetById(history.TransactionId).Sequence < transaction.Sequence) + 1;

    /// <summary>
    /// Gets the existing Fund Balance for the specified Fund ID as of the provided date and sequence number
    /// </summary>
    private FundBalance GetExistingFundBalanceAsOf(FundId fundId, DateOnly asOfDate, int asOfSequence)
    {
        FundBalanceHistory? existingHistory = fundBalanceHistoryRepository.GetLatestHistoryEarlierThan(fundId, asOfDate, asOfSequence);
        if (existingHistory != null)
        {
            return existingHistory.ToFundBalance();
        }
        return new FundBalance(fundId, fundRepository.GetById(fundId).OnboardedBalance ?? 0, 0, 0);
    }
}