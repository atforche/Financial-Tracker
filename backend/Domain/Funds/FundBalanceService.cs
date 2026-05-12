using Domain.Accounts;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Fund Balances
/// </summary>
public class FundBalanceService(
    IFundRepository fundRepository,
    IFundBalanceHistoryRepository fundBalanceHistoryRepository,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Gets the current balance for the provided Account
    /// </summary>
    public FundBalance GetCurrentBalance(FundId fundId) =>
        fundBalanceHistoryRepository.GetLatestForFund(fundId)?.ToFundBalance() ??
            new FundBalance(fundId, fundRepository.GetById(fundId).OnboardedBalance ?? 0, 0, 0, 0, 0);

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
    /// Updates the Fund Balances for an updated Transaction
    /// </summary>
    internal void UpdateTransaction(Transaction transaction)
    {
        foreach (FundId fund in transaction.GetAllAffectedFundIds(null))
        {
            UpdateExistingBalanceHistory(transaction, fund);
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
                UpdateExistingBalanceHistory(transaction, fund);
            }
            else
            {
                AddNewBalanceHistory(transaction, fund, postedDate.Value);
            }
        }
    }

    /// <summary>
    /// Updates the Fund Balances for an unposted Transaction
    /// </summary>
    internal void UnpostTransaction(Transaction transaction)
    {
        foreach (AccountId accountId in transaction.GetAllAffectedAccountIds())
        {
            IEnumerable<FundId> affectedFunds = transaction.GetAllAffectedFundIds(accountId);
            foreach (FundId fund in affectedFunds)
            {
                FundBalanceHistory? oldPostedHistory = fundBalanceHistoryRepository
                    .GetAllByTransactionId(transaction.Id)
                    .SingleOrDefault(bh => bh.FundId == fund && bh.Date != transaction.Date);
                if (oldPostedHistory == null)
                {
                    UpdateExistingBalanceHistory(transaction, fund);
                }
                else
                {
                    DeleteExistingBalanceHistory(transaction, oldPostedHistory);
                    fundBalanceHistoryRepository.Delete(oldPostedHistory);
                }
            }
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
        int sequence = fundBalanceHistoryRepository.GetNextSequenceForFundAndDate(fund, date);
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
            if (transactionRepository.GetById(history.TransactionId).AccountingPeriodId != transaction.AccountingPeriodId)
            {
                updatedBalance = new FundBalance(
                    history.FundId,
                    updatedBalance.PostedBalance,
                    existingBalance.AmountAssigned,
                    existingBalance.PendingAmountAssigned,
                    existingBalance.AmountSpent,
                    existingBalance.PendingAmountSpent);
            }
            history.Update(updatedBalance);
        }
        fundBalanceHistoryRepository.Add(newBalanceHistory);
    }

    /// <summary>
    /// Updates an existing Fund Balance History entry
    /// </summary>
    private void UpdateExistingBalanceHistory(Transaction transaction, FundId fund)
    {
        FundBalanceHistory existingHistory = fundBalanceHistoryRepository.GetEarliestByTransactionId(fund, transaction.Id);
        FundBalance existingBalance = GetExistingFundBalanceAsOf(fund, existingHistory.Date, existingHistory.Sequence);
        existingHistory.Update(transaction.ApplyToFundBalance(existingBalance, existingHistory.Date));

        foreach (FundBalanceHistory history in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(existingHistory.FundId, existingHistory.Date, existingHistory.Sequence))
        {
            existingBalance = history.ToFundBalance();
            FundBalance updatedBalance = transaction.ApplyToFundBalance(existingBalance, existingHistory.Date);
            if (transactionRepository.GetById(history.TransactionId).AccountingPeriodId != transaction.AccountingPeriodId)
            {
                updatedBalance = new FundBalance(
                    history.FundId,
                    updatedBalance.PostedBalance,
                    existingBalance.AmountAssigned,
                    existingBalance.PendingAmountAssigned,
                    existingBalance.AmountSpent,
                    existingBalance.PendingAmountSpent);
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
    /// Gets the existing Fund Balance for the specified Fund ID as of the provided date and sequence number
    /// </summary>
    private FundBalance GetExistingFundBalanceAsOf(FundId fundId, DateOnly asOfDate, int asOfSequence)
    {
        FundBalanceHistory? existingHistory = fundBalanceHistoryRepository.GetLatestHistoryEarlierThan(fundId, asOfDate, asOfSequence);
        if (existingHistory != null)
        {
            return existingHistory.ToFundBalance();
        }
        return new FundBalance(fundId, fundRepository.GetById(fundId).OnboardedBalance ?? 0, 0, 0, 0, 0);
    }
}