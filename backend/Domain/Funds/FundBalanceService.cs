using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Fund Balances
/// </summary>
public class FundBalanceService(IFundBalanceHistoryRepository fundBalanceHistoryRepository)
{
    /// <summary>
    /// Gets the current balance for the provided Account
    /// </summary>
    public FundBalance GetCurrentBalance(FundId fundId) =>
        fundBalanceHistoryRepository.GetLatestForFund(fundId)?.ToFundBalance() ?? new FundBalance(fundId, [], [], []);

    /// <summary>
    /// Gets the Fund Balances prior to the provided Transaction
    /// </summary>
    public IEnumerable<FundBalance> GetPreviousBalancesForTransaction(Transaction transaction) =>
        GetAllAffectedFunds(transaction)
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
        GetAllAffectedFunds(transaction)
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
        foreach (FundId fund in GetAllAffectedFunds(newTransaction))
        {
            AddNewBalanceHistory(newTransaction, fund, newTransaction.Date);
        }
    }

    /// <summary>
    /// Updates the Fund Balances for an updated Transaction
    /// </summary>
    internal void UpdateTransaction(Transaction transaction)
    {
        foreach (FundId fund in GetAllAffectedFunds(transaction))
        {
            UpdateExistingBalanceHistory(transaction, fund);
        }
    }

    /// <summary>
    /// Updates the Account Balances for a newly posted Transaction
    /// </summary>
    internal void PostTransaction(Transaction transaction, TransactionAccount transactionAccount)
    {
        if (transactionAccount.PostedDate == null)
        {
            return;
        }
        if (transactionAccount.PostedDate == transaction.Date)
        {
            foreach (FundAmount fundAmount in transactionAccount.FundAmounts)
            {
                UpdateExistingBalanceHistory(transaction, fundAmount.FundId);
            }
        }
        foreach (FundAmount fundAmount in transactionAccount.FundAmounts)
        {
            AddNewBalanceHistory(transaction, fundAmount.FundId, transactionAccount.PostedDate.Value);
        }
    }

    /// <summary>
    /// Updates the Fund Balances for a deleted Transaction
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (FundBalanceHistory balanceHistory in fundBalanceHistoryRepository.GetAllByTransactionId(transaction.Id))
        {
            DeleteExistingBalanceHistory(balanceHistory);
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
        FundBalance newBalance = transaction.ApplyToFundBalance(existingBalance, date);
        var newBalanceHistory = new FundBalanceHistory(newBalance.FundId,
            transaction.Id,
            date,
            sequence,
            newBalance.AccountBalances,
            newBalance.PendingDebits,
            newBalance.PendingCredits);

        foreach ((FundBalanceHistory history, Transaction existingTransaction) in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(newBalanceHistory.FundId, newBalanceHistory.Date, newBalanceHistory.Sequence))
        {
            if (history.Date == newBalanceHistory.Date)
            {
                history.Sequence += 1;
            }
            FundBalance updatedBalance = existingTransaction.ApplyToFundBalance(newBalance, history.Date);
            history.AccountBalances = updatedBalance.AccountBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            newBalance = updatedBalance;
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
        FundBalance newBalance = transaction.ApplyToFundBalance(existingBalance, existingHistory.Date);
        existingHistory.AccountBalances = newBalance.AccountBalances;
        existingHistory.PendingDebits = newBalance.PendingDebits;
        existingHistory.PendingCredits = newBalance.PendingCredits;

        foreach ((FundBalanceHistory history, Transaction existingTransaction) in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(existingHistory.FundId, existingHistory.Date, existingHistory.Sequence))
        {
            FundBalance updatedBalance = existingTransaction.ApplyToFundBalance(newBalance, history.Date);
            history.AccountBalances = updatedBalance.AccountBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            newBalance = updatedBalance;
        }
    }

    /// <summary>
    /// Deletes an existing Fund Balance History entry
    /// </summary>
    private void DeleteExistingBalanceHistory(FundBalanceHistory deletedBalanceHistory)
    {
        FundBalance existingBalance = GetExistingFundBalanceAsOf(deletedBalanceHistory.FundId, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence);
        foreach ((FundBalanceHistory history, Transaction transaction) in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(deletedBalanceHistory.FundId, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence + 1))
        {
            if (history.Date == deletedBalanceHistory.Date)
            {
                history.Sequence -= 1;
            }
            FundBalance updatedBalance = transaction.ApplyToFundBalance(existingBalance, history.Date);
            history.AccountBalances = updatedBalance.AccountBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            existingBalance = updatedBalance;
        }
    }

    /// <summary>
    /// Gets all Fund IDs affected by the provided Transaction
    /// </summary>
    private static HashSet<FundId> GetAllAffectedFunds(Transaction transaction)
    {
        HashSet<FundId> funds = [];
        if (transaction.DebitAccount != null)
        {
            funds.UnionWith(transaction.DebitAccount.FundAmounts.Select(fundAmount => fundAmount.FundId));
        }
        if (transaction.CreditAccount != null)
        {
            funds.UnionWith(transaction.CreditAccount.FundAmounts.Select(fundAmount => fundAmount.FundId));
        }
        return funds;
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
        return new FundBalance(fundId, [], [], []);
    }
}