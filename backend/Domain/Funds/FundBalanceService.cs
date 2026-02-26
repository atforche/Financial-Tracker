using Domain.Transactions;
using Domain.Transactions.Exceptions;

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
    /// Attempts to update the Fund Balances for a newly added Transaction
    /// </summary>
    internal bool TryAddTransaction(Transaction newTransaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        foreach (FundId fund in GetAllAffectedFunds(newTransaction))
        {
            int sequence = fundBalanceHistoryRepository.GetNextSequenceForFundAndDate(fund, newTransaction.Date);
            FundBalance existingBalance = GetExistingFundBalanceAsOf(fund, newTransaction.Date, sequence);
            if (!newTransaction.TryApplyToFundBalance(existingBalance, newTransaction.Date, out FundBalance? newBalance, out IEnumerable<Exception> balanceExceptions))
            {
                exceptions = exceptions.Concat(balanceExceptions);
                return false;
            }
            var newBalanceHistory = new FundBalanceHistory(newBalance.FundId,
                newTransaction.Id,
                newTransaction.Date,
                sequence,
                newBalance.AccountBalances,
                newBalance.PendingDebits,
                newBalance.PendingCredits);
            if (!TryAddNewBalanceHistory(newBalanceHistory, out IEnumerable<Exception> balanceHistoryExceptions))
            {
                exceptions = exceptions.Concat(balanceHistoryExceptions);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to update the Fund Balances for an updated Transaction
    /// </summary>
    internal bool TryUpdateTransaction(Transaction transaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        foreach (FundId fund in GetAllAffectedFunds(transaction))
        {
            FundBalanceHistory existingHistory = fundBalanceHistoryRepository.GetEarliestByTransactionId(fund, transaction.Id);
            FundBalance previousBalance = GetExistingFundBalanceAsOf(fund, existingHistory.Date, existingHistory.Sequence);
            if (!transaction.TryApplyToFundBalance(previousBalance, existingHistory.Date, out FundBalance? updatedDebitBalance, out IEnumerable<Exception> balanceExceptions))
            {
                exceptions = exceptions.Concat(balanceExceptions);
                return false;
            }
            existingHistory.AccountBalances = updatedDebitBalance.AccountBalances;
            existingHistory.PendingDebits = updatedDebitBalance.PendingDebits;
            existingHistory.PendingCredits = updatedDebitBalance.PendingCredits;
            if (!TryUpdateExistingBalanceHistory(existingHistory, out IEnumerable<Exception> updateExceptions))
            {
                exceptions = exceptions.Concat(updateExceptions);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to update the Account Balances for a newly posted Transaction
    /// </summary>
    internal bool TryPostTransaction(Transaction transaction, TransactionAccount transactionAccount, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (transactionAccount.PostedDate == null)
        {
            exceptions = exceptions.Append(new InvalidTransactionDateException("Transaction Account must have a Posted Date to post a transaction."));
            return false;
        }
        if (transactionAccount.PostedDate == transaction.Date)
        {
            foreach (FundAmount fundAmount in transactionAccount.FundAmounts)
            {
                FundBalanceHistory existingHistory = fundBalanceHistoryRepository.GetEarliestByTransactionId(fundAmount.FundId, transaction.Id);
                FundBalance previousBalance = GetExistingFundBalanceAsOf(fundAmount.FundId, existingHistory.Date, existingHistory.Sequence);
                if (!transaction.TryApplyToFundBalance(previousBalance, transaction.Date, out FundBalance? updatedBalance, out IEnumerable<Exception> updatedBalanceExceptions))
                {
                    exceptions = exceptions.Concat(updatedBalanceExceptions);
                    return false;
                }
                existingHistory.AccountBalances = updatedBalance.AccountBalances;
                existingHistory.PendingDebits = updatedBalance.PendingDebits;
                existingHistory.PendingCredits = updatedBalance.PendingCredits;
                if (!TryUpdateExistingBalanceHistory(existingHistory, out IEnumerable<Exception> updateExceptions))
                {
                    exceptions = exceptions.Concat(updateExceptions);
                    return false;
                }
            }
            return true;
        }
        foreach (FundAmount fundAmount in transactionAccount.FundAmounts)
        {
            int newSequence = fundBalanceHistoryRepository.GetNextSequenceForFundAndDate(fundAmount.FundId, transactionAccount.PostedDate.Value);
            FundBalance existingBalance = GetExistingFundBalanceAsOf(fundAmount.FundId, transactionAccount.PostedDate.Value, newSequence);
            if (!transaction.TryApplyToFundBalance(existingBalance, transactionAccount.PostedDate.Value, out FundBalance? newBalance, out IEnumerable<Exception> balanceExceptions))
            {
                exceptions = exceptions.Concat(balanceExceptions);
                return false;
            }
            var newBalanceHistory = new FundBalanceHistory(newBalance.FundId,
                transaction.Id,
                transactionAccount.PostedDate.Value,
                newSequence,
                newBalance.AccountBalances,
                newBalance.PendingDebits,
                newBalance.PendingCredits);
            if (!TryAddNewBalanceHistory(newBalanceHistory, out IEnumerable<Exception> balanceHistoryExceptions))
            {
                exceptions = exceptions.Concat(balanceHistoryExceptions);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to update the Fund Balances for a deleted Transaction
    /// </summary>
    internal bool TryDeleteTransaction(Transaction transaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        foreach (FundBalanceHistory balanceHistory in fundBalanceHistoryRepository.GetAllByTransactionId(transaction.Id))
        {
            if (!TryDeleteExistingBalanceHistory(balanceHistory, out IEnumerable<Exception> deleteExceptions))
            {
                exceptions = exceptions.Concat(deleteExceptions);
                return false;
            }
            fundBalanceHistoryRepository.Delete(balanceHistory);
        }
        return true;
    }

    /// <summary>
    /// Attempts to add a new Fund Balance History entry
    /// </summary>
    private bool TryAddNewBalanceHistory(FundBalanceHistory newBalanceHistory, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        var existingBalance = newBalanceHistory.ToFundBalance();
        foreach ((FundBalanceHistory history, Transaction transaction) in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(newBalanceHistory.FundId, newBalanceHistory.Date, newBalanceHistory.Sequence))
        {
            if (history.Date == newBalanceHistory.Date)
            {
                history.Sequence += 1;
            }
            if (!transaction.TryApplyToFundBalance(existingBalance, history.Date, out FundBalance? updatedBalance, out IEnumerable<Exception> updateExceptions))
            {
                exceptions = exceptions.Concat(updateExceptions);
                return false;
            }
            history.AccountBalances = updatedBalance.AccountBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            existingBalance = updatedBalance;
        }
        fundBalanceHistoryRepository.Add(newBalanceHistory);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund Balance History entry
    /// </summary>
    private bool TryUpdateExistingBalanceHistory(FundBalanceHistory updatedBalanceHistory, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        var existingBalance = updatedBalanceHistory.ToFundBalance();
        foreach ((FundBalanceHistory history, Transaction transaction) in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(updatedBalanceHistory.FundId, updatedBalanceHistory.Date, updatedBalanceHistory.Sequence))
        {
            if (!transaction.TryApplyToFundBalance(existingBalance, history.Date, out FundBalance? updatedBalance, out IEnumerable<Exception> updateExceptions))
            {
                exceptions = exceptions.Concat(updateExceptions);
                return false;
            }
            history.AccountBalances = updatedBalance.AccountBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            existingBalance = updatedBalance;
        }
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Fund Balance History entry
    /// </summary>
    private bool TryDeleteExistingBalanceHistory(FundBalanceHistory deletedBalanceHistory, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        FundBalance existingBalance = GetExistingFundBalanceAsOf(deletedBalanceHistory.FundId, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence);
        foreach ((FundBalanceHistory history, Transaction transaction) in fundBalanceHistoryRepository
            .GetAllHistoriesLaterThan(deletedBalanceHistory.FundId, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence + 1))
        {
            if (history.Date == deletedBalanceHistory.Date)
            {
                history.Sequence -= 1;
            }
            if (!transaction.TryApplyToFundBalance(existingBalance, history.Date, out FundBalance? updatedBalance, out IEnumerable<Exception> updateExceptions))
            {
                exceptions = exceptions.Concat(updateExceptions);
                return false;
            }
            history.AccountBalances = updatedBalance.AccountBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            existingBalance = updatedBalance;
        }
        return true;
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