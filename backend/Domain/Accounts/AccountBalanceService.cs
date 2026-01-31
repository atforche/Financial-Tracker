using Domain.Transactions;
using Domain.Transactions.Exceptions;

namespace Domain.Accounts;

/// <summary>
/// Service for managing Account Balances
/// </summary>
public class AccountBalanceService(IAccountBalanceHistoryRepository accountBalanceHistoryRepository)
{
    /// <summary>
    /// Gets the current balance for the provided Account
    /// </summary>
    public AccountBalance GetCurrentBalance(AccountId accountId) => accountBalanceHistoryRepository.FindLatestForAccount(accountId).ToAccountBalance();

    /// <summary>
    /// Attempts to update the Account Balances for a newly added Transaction
    /// </summary>
    internal bool TryAddTransaction(Transaction newTransaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (newTransaction.DebitAccount != null)
        {
            int debitSequence = accountBalanceHistoryRepository.GetNextSequenceForAccountAndDate(newTransaction.DebitAccount.Account, newTransaction.Date);
            AccountBalance existingDebitBalance = GetExistingAccountBalanceAsOf(newTransaction.DebitAccount.Account, newTransaction.Date, debitSequence);
            if (!newTransaction.TryApplyToAccountBalance(existingDebitBalance, newTransaction.Date, out AccountBalance? newDebitBalance, out IEnumerable<Exception> debitExceptions))
            {
                exceptions = exceptions.Concat(debitExceptions);
                return false;
            }
            var newDebitBalanceHistory = new AccountBalanceHistory(newDebitBalance.AccountId,
                newTransaction.Id,
                newTransaction.Date,
                debitSequence,
                newDebitBalance.FundBalances,
                newDebitBalance.PendingDebits,
                newDebitBalance.PendingCredits);
            if (!TryAddNewBalanceHistory(newDebitBalanceHistory, out IEnumerable<Exception> balanceHistoryExceptions))
            {
                exceptions = exceptions.Concat(balanceHistoryExceptions);
                return false;
            }
        }
        if (newTransaction.CreditAccount != null)
        {
            int creditSequence = accountBalanceHistoryRepository.GetNextSequenceForAccountAndDate(newTransaction.CreditAccount.Account, newTransaction.Date);
            AccountBalance existingCreditBalance = GetExistingAccountBalanceAsOf(newTransaction.CreditAccount.Account, newTransaction.Date, creditSequence);
            if (!newTransaction.TryApplyToAccountBalance(existingCreditBalance, newTransaction.Date, out AccountBalance? newCreditBalance, out IEnumerable<Exception> creditExceptions))
            {
                exceptions = exceptions.Concat(creditExceptions);
                return false;
            }
            var newCreditBalanceHistory = new AccountBalanceHistory(newCreditBalance.AccountId,
                newTransaction.Id,
                newTransaction.Date,
                creditSequence,
                newCreditBalance.FundBalances,
                newCreditBalance.PendingDebits,
                newCreditBalance.PendingCredits);
            if (!TryAddNewBalanceHistory(newCreditBalanceHistory, out IEnumerable<Exception> balanceHistoryExceptions))
            {
                exceptions = exceptions.Concat(balanceHistoryExceptions);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to update the Account Balances for an updated Transaction
    /// </summary>
    internal bool TryUpdateTransaction(Transaction transaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (transaction.DebitAccount != null)
        {
            AccountBalanceHistory existingDebitHistory = accountBalanceHistoryRepository.FindEarliestByTransactionId(transaction.DebitAccount.Account, transaction.Id);
            AccountBalance previousBalance = GetExistingAccountBalanceAsOf(transaction.DebitAccount.Account, existingDebitHistory.Date, existingDebitHistory.Sequence);
            if (!transaction.TryApplyToAccountBalance(previousBalance, existingDebitHistory.Date, out AccountBalance? updatedDebitBalance, out IEnumerable<Exception> debitExceptions))
            {
                exceptions = exceptions.Concat(debitExceptions);
                return false;
            }
            existingDebitHistory.FundBalances = updatedDebitBalance.FundBalances;
            existingDebitHistory.PendingDebits = updatedDebitBalance.PendingDebits;
            existingDebitHistory.PendingCredits = updatedDebitBalance.PendingCredits;
            if (!TryUpdateExistingBalanceHistory(existingDebitHistory, out IEnumerable<Exception> updateExceptions))
            {
                exceptions = exceptions.Concat(updateExceptions);
                return false;
            }
        }
        if (transaction.CreditAccount != null)
        {
            AccountBalanceHistory existingCreditHistory = accountBalanceHistoryRepository.FindEarliestByTransactionId(transaction.CreditAccount.Account, transaction.Id);
            AccountBalance previousBalance = GetExistingAccountBalanceAsOf(transaction.CreditAccount.Account, existingCreditHistory.Date, existingCreditHistory.Sequence);
            if (!transaction.TryApplyToAccountBalance(previousBalance, existingCreditHistory.Date, out AccountBalance? updatedCreditBalance, out IEnumerable<Exception> creditExceptions))
            {
                exceptions = exceptions.Concat(creditExceptions);
                return false;
            }
            existingCreditHistory.FundBalances = updatedCreditBalance.FundBalances;
            existingCreditHistory.PendingDebits = updatedCreditBalance.PendingDebits;
            existingCreditHistory.PendingCredits = updatedCreditBalance.PendingCredits;
            if (!TryUpdateExistingBalanceHistory(existingCreditHistory, out IEnumerable<Exception> updateExceptions))
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
            AccountBalanceHistory existingHistory = accountBalanceHistoryRepository.FindEarliestByTransactionId(transactionAccount.Account, transaction.Id);
            AccountBalance previousBalance = GetExistingAccountBalanceAsOf(transactionAccount.Account, existingHistory.Date, existingHistory.Sequence);
            if (!transaction.TryApplyToAccountBalance(previousBalance, transaction.Date, out AccountBalance? updatedBalance, out IEnumerable<Exception> updatedBalanceExceptions))
            {
                exceptions = exceptions.Concat(updatedBalanceExceptions);
                return false;
            }
            existingHistory.FundBalances = updatedBalance.FundBalances;
            existingHistory.PendingDebits = updatedBalance.PendingDebits;
            existingHistory.PendingCredits = updatedBalance.PendingCredits;
            if (!TryUpdateExistingBalanceHistory(existingHistory, out IEnumerable<Exception> updateExceptions))
            {
                exceptions = exceptions.Concat(updateExceptions);
                return false;
            }
            return true;
        }
        int newSequence = accountBalanceHistoryRepository.GetNextSequenceForAccountAndDate(transactionAccount.Account, transactionAccount.PostedDate.Value);
        AccountBalance existingBalance = GetExistingAccountBalanceAsOf(transactionAccount.Account, transactionAccount.PostedDate.Value, newSequence);
        if (!transaction.TryApplyToAccountBalance(existingBalance, transactionAccount.PostedDate.Value, out AccountBalance? newBalance, out IEnumerable<Exception> balanceExceptions))
        {
            exceptions = exceptions.Concat(balanceExceptions);
            return false;
        }
        var newBalanceHistory = new AccountBalanceHistory(newBalance.AccountId,
            transaction.Id,
            transactionAccount.PostedDate.Value,
            newSequence,
            newBalance.FundBalances,
            newBalance.PendingDebits,
            newBalance.PendingCredits);
        if (!TryAddNewBalanceHistory(newBalanceHistory, out IEnumerable<Exception> balanceHistoryExceptions))
        {
            exceptions = exceptions.Concat(balanceHistoryExceptions);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to update the Account Balances for a deleted Transaction
    /// </summary>
    public bool TryDeleteTransaction(Transaction transaction, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        foreach (AccountBalanceHistory balanceHistory in accountBalanceHistoryRepository.GetAllByTransactionId(transaction.Id))
        {
            if (!TryDeleteExistingBalanceHistory(balanceHistory, out IEnumerable<Exception> deleteExceptions))
            {
                exceptions = exceptions.Concat(deleteExceptions);
                return false;
            }
            accountBalanceHistoryRepository.Delete(balanceHistory);
        }
        return true;
    }

    /// <summary>
    /// Attempts to add a new Account Balance History entry
    /// </summary>
    private bool TryAddNewBalanceHistory(AccountBalanceHistory newBalanceHistory, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        var existingBalance = newBalanceHistory.ToAccountBalance();
        foreach ((AccountBalanceHistory history, Transaction transaction) in accountBalanceHistoryRepository
            .FindAllHistoriesLaterThanOrEqualTo(newBalanceHistory.AccountId, newBalanceHistory.Date, newBalanceHistory.Sequence))
        {
            if (history.Date == newBalanceHistory.Date)
            {
                history.Sequence += 1;
            }
            if (!transaction.TryApplyToAccountBalance(existingBalance, history.Date, out AccountBalance? updatedBalance, out IEnumerable<Exception> updateExceptions))
            {
                exceptions = exceptions.Concat(updateExceptions);
                return false;
            }
            history.FundBalances = updatedBalance.FundBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            existingBalance = updatedBalance;
        }
        accountBalanceHistoryRepository.Add(newBalanceHistory);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Account Balance History entry
    /// </summary>
    private bool TryUpdateExistingBalanceHistory(AccountBalanceHistory updatedBalanceHistory, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        var existingBalance = updatedBalanceHistory.ToAccountBalance();
        foreach ((AccountBalanceHistory history, Transaction transaction) in accountBalanceHistoryRepository
            .FindAllHistoriesLaterThanOrEqualTo(updatedBalanceHistory.AccountId, updatedBalanceHistory.Date, updatedBalanceHistory.Sequence))
        {
            if (!transaction.TryApplyToAccountBalance(existingBalance, history.Date, out AccountBalance? updatedBalance, out IEnumerable<Exception> updateExceptions))
            {
                exceptions = exceptions.Concat(updateExceptions);
                return false;
            }
            history.FundBalances = updatedBalance.FundBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            existingBalance = updatedBalance;
        }
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Account Balance History entry
    /// </summary>
    private bool TryDeleteExistingBalanceHistory(AccountBalanceHistory deletedBalanceHistory, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        AccountBalance existingBalance = GetExistingAccountBalanceAsOf(deletedBalanceHistory.AccountId, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence);
        foreach ((AccountBalanceHistory history, Transaction transaction) in accountBalanceHistoryRepository
            .FindAllHistoriesLaterThanOrEqualTo(deletedBalanceHistory.AccountId, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence + 1))
        {
            if (history.Date == deletedBalanceHistory.Date)
            {
                history.Sequence -= 1;
            }
            if (!transaction.TryApplyToAccountBalance(existingBalance, history.Date, out AccountBalance? updatedBalance, out IEnumerable<Exception> updateExceptions))
            {
                exceptions = exceptions.Concat(updateExceptions);
                return false;
            }
            history.FundBalances = updatedBalance.FundBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            existingBalance = updatedBalance;
        }
        return true;
    }

    /// <summary>
    /// Gets the existing Account Balance for the specified Account ID as of the provided date and sequence number
    /// </summary>
    private AccountBalance GetExistingAccountBalanceAsOf(AccountId accountId, DateOnly asOfDate, int asOfSequence)
    {
        AccountBalanceHistory? existingHistory = accountBalanceHistoryRepository.FindLatestHistoryEarlierThan(accountId, asOfDate, asOfSequence);
        if (existingHistory != null)
        {
            return existingHistory.ToAccountBalance();
        }
        return new AccountBalance(accountId, [], [], []);
    }
}