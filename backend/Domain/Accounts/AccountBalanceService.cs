using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Service for managing Account Balances
/// </summary>
public class AccountBalanceService(
    IAccountRepository accountRepository,
    IAccountBalanceHistoryRepository accountBalanceHistoryRepository)
{
    /// <summary>
    /// Gets the current balance for the provided Account
    /// </summary>
    public AccountBalance GetCurrentBalance(Account account) =>
        accountBalanceHistoryRepository.GetLatestForAccount(account.Id)?.ToAccountBalance() ?? new AccountBalance(account, 0, 0, 0);

    /// <summary>
    /// Gets the Account Balance prior to the provided Transaction
    /// </summary>
    public AccountBalance GetPreviousBalanceForTransaction(Transaction transaction, AccountId account)
    {
        var balanceHistories = accountBalanceHistoryRepository.GetAllByTransactionId(transaction.Id).ToList();
        DateOnly? postedDate = transaction.GetPostedDateForAccount(account);
        if (postedDate != null)
        {
            AccountBalanceHistory postedHistory = balanceHistories.First(bh => bh.Date == postedDate);
            return GetExistingAccountBalanceAsOf(
                accountRepository.GetById(account),
                postedHistory.Date,
                postedHistory.Sequence);
        }
        AccountBalanceHistory earliestHistory = balanceHistories.OrderBy(bh => bh.Date).ThenBy(bh => bh.Sequence).First();
        return GetExistingAccountBalanceAsOf(
            accountRepository.GetById(account),
            earliestHistory.Date,
            earliestHistory.Sequence);
    }

    /// <summary>
    /// Gets the Account Balance after the provided Transaction
    /// </summary>
    public AccountBalance GetNewBalanceForTransaction(Transaction transaction, AccountId account)
    {
        var balanceHistories = accountBalanceHistoryRepository.GetAllByTransactionId(transaction.Id).ToList();
        DateOnly? postedDate = transaction.GetPostedDateForAccount(account);
        if (postedDate != null)
        {
            return balanceHistories.Single(bh => bh.Date == postedDate).ToAccountBalance();
        }
        return balanceHistories.OrderBy(bh => bh.Date).ThenBy(bh => bh.Sequence).First().ToAccountBalance();
    }

    /// <summary>
    /// Updates the Account Balances for a newly added Transaction
    /// </summary>
    internal void AddTransaction(Transaction newTransaction)
    {
        foreach (AccountId accountId in newTransaction.GetAllAffectedAccountIds())
        {
            AddNewBalanceHistory(newTransaction, accountId, newTransaction.Date);
        }
    }

    /// <summary>
    /// Updates the Account Balances for an updated Transaction
    /// </summary>
    internal void UpdateTransaction(Transaction transaction)
    {
        foreach (AccountId accountId in transaction.GetAllAffectedAccountIds())
        {
            UpdateExistingBalanceHistory(transaction, accountId);
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
        if (postedDate == transaction.Date)
        {
            UpdateExistingBalanceHistory(transaction, accountId);
        }
        else
        {
            AddNewBalanceHistory(transaction, accountId, postedDate.Value);
        }
    }

    /// <summary>
    /// Updates the Account Balances for an unposted Transaction
    /// </summary>
    internal void UnpostTransaction(Transaction transaction)
    {
        foreach (AccountId accountId in transaction.GetAllAffectedAccountIds())
        {
            AccountBalanceHistory? oldPostedHistory = accountBalanceHistoryRepository
                .GetAllByTransactionId(transaction.Id)
                .SingleOrDefault(bh => bh.Date != transaction.Date);
            if (oldPostedHistory == null)
            {
                UpdateExistingBalanceHistory(transaction, accountId);
            }
            else
            {
                DeleteExistingBalanceHistory(transaction, oldPostedHistory);
                accountBalanceHistoryRepository.Delete(oldPostedHistory);
            }
        }
    }

    /// <summary>
    /// Updates the Account Balances for a deleted Transaction
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (AccountBalanceHistory balanceHistory in accountBalanceHistoryRepository.GetAllByTransactionId(transaction.Id))
        {
            DeleteExistingBalanceHistory(transaction, balanceHistory);
            accountBalanceHistoryRepository.Delete(balanceHistory);
        }
    }

    /// <summary>
    /// Adds a new Account Balance History entry
    /// </summary>
    private void AddNewBalanceHistory(Transaction transaction, AccountId accountId, DateOnly date)
    {
        int sequence = accountBalanceHistoryRepository.GetNextSequenceForAccountAndDate(accountId, date);
        Account account = accountRepository.GetById(accountId);
        AccountBalance existingBalance = GetExistingAccountBalanceAsOf(account, date, sequence);
        var newBalanceHistory = new AccountBalanceHistory(
            account,
            transaction.Id,
            date,
            sequence,
            transaction.ApplyToAccountBalance(existingBalance, date));

        foreach (AccountBalanceHistory history in accountBalanceHistoryRepository
            .GetAllHistoriesLaterThan(newBalanceHistory.Account.Id, newBalanceHistory.Date, newBalanceHistory.Sequence))
        {
            if (history.Date == newBalanceHistory.Date)
            {
                history.Sequence += 1;
            }
            AccountBalance updatedBalance = transaction.ApplyToAccountBalance(history.ToAccountBalance(), date);
            history.Update(updatedBalance);
        }
        accountBalanceHistoryRepository.Add(newBalanceHistory);
    }

    /// <summary>
    /// Updates an existing Account Balance History entry
    /// </summary>
    private void UpdateExistingBalanceHistory(Transaction transaction, AccountId accountId)
    {
        AccountBalanceHistory existingHistory = accountBalanceHistoryRepository.GetEarliestByTransactionId(accountId, transaction.Id);
        AccountBalance existingBalance = GetExistingAccountBalanceAsOf(
            existingHistory.Account,
            existingHistory.Date,
            existingHistory.Sequence);
        existingHistory.Update(transaction.ApplyToAccountBalance(existingBalance, existingHistory.Date));

        foreach (AccountBalanceHistory history in accountBalanceHistoryRepository
            .GetAllHistoriesLaterThan(existingHistory.Account.Id, existingHistory.Date, existingHistory.Sequence))
        {
            AccountBalance updatedBalance = transaction.ApplyToAccountBalance(history.ToAccountBalance(), history.Date);
            history.Update(updatedBalance);
        }
    }

    /// <summary>
    /// Deletes an existing Account Balance History entry
    /// </summary>
    private void DeleteExistingBalanceHistory(Transaction transaction, AccountBalanceHistory deletedBalanceHistory)
    {
        foreach (AccountBalanceHistory history in accountBalanceHistoryRepository
            .GetAllHistoriesLaterThan(deletedBalanceHistory.Account.Id, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence + 1))
        {
            if (history.Date == deletedBalanceHistory.Date)
            {
                history.Sequence -= 1;
            }
            AccountBalance updatedBalance = transaction.ApplyToAccountBalance(history.ToAccountBalance(), deletedBalanceHistory.Date, reverse: true);
            history.Update(updatedBalance);
        }
    }

    /// <summary>
    /// Gets the existing Account Balance for the specified Account as of the provided date and sequence number
    /// </summary>
    private AccountBalance GetExistingAccountBalanceAsOf(Account account, DateOnly asOfDate, int asOfSequence)
    {
        AccountBalanceHistory? existingHistory = accountBalanceHistoryRepository.GetLatestHistoryEarlierThan(account.Id, asOfDate, asOfSequence);
        if (existingHistory != null)
        {
            return existingHistory.ToAccountBalance();
        }
        return new AccountBalance(account, 0, 0, 0);
    }
}