using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Service for managing Account Balances
/// </summary>
public class AccountBalanceService(
    IAccountRepository accountRepository,
    IAccountBalanceHistoryRepository accountBalanceHistoryRepository,
    ITransactionRepository transactionRepository,
    PendingAccountBalanceService pendingAccountBalanceService)
{
    /// <summary>
    /// Gets the current balance for the provided Account
    /// </summary>
    public AccountBalance GetCurrentBalance(Account account)
    {
        AccountBalanceHistory? latestHistory = accountBalanceHistoryRepository.GetLatestForAccount(account.Id);
        var postedBalance = new AccountBalance(
            account,
            latestHistory?.PostedBalance ?? account.OnboardedBalance ?? 0);
        return pendingAccountBalanceService.ApplyPendingEffects(postedBalance);
    }

    /// <summary>
    /// Gets the Account Balance prior to the provided Transaction
    /// </summary>
    public AccountBalance GetPreviousBalanceForTransaction(Transaction transaction, AccountId account)
    {
        var balanceHistories = accountBalanceHistoryRepository.GetAllByTransactionIdAndAccountId(transaction.Id, account).ToList();
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
        var balanceHistories = accountBalanceHistoryRepository.GetAllByTransactionIdAndAccountId(transaction.Id, account).ToList();
        DateOnly? postedDate = transaction.GetPostedDateForAccount(account);
        if (postedDate != null)
        {
            return balanceHistories.Single(bh => bh.Date == postedDate).ToAccountBalance();
        }
        return balanceHistories.OrderBy(bh => bh.Date).ThenBy(bh => bh.Sequence).First().ToAccountBalance();
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
        RemoveBalanceHistories(transaction, accountId);
        AddNewBalanceHistory(transaction, accountId, postedDate.Value);
    }

    /// <summary>
    /// Updates the Account Balances for an unposted Transaction
    /// </summary>
    internal void UnpostTransaction(Transaction transaction)
    {
        foreach (AccountId accountId in transaction.GetAllAffectedAccountIds())
        {
            RemoveBalanceHistories(transaction, accountId);
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
        int sequence = GetSequenceForTransaction(accountId, transaction, date);
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
    /// Removes all Account Balance History entries for a Transaction and Account.
    /// </summary>
    private void RemoveBalanceHistories(Transaction transaction, AccountId accountId)
    {
        foreach (AccountBalanceHistory balanceHistory in accountBalanceHistoryRepository
            .GetAllByTransactionIdAndAccountId(transaction.Id, accountId)
            .ToList())
        {
            DeleteExistingBalanceHistory(transaction, balanceHistory);
            accountBalanceHistoryRepository.Delete(balanceHistory);
        }
    }

    /// <summary>
    /// Gets the balance history sequence number for the provided Transaction.
    /// </summary>
    /// <remarks>
    /// This keeps rebuilt balance history entries in transaction-sequence order when an updated transaction is removed and re-added on the same date.
    /// </remarks>
    private int GetSequenceForTransaction(AccountId accountId, Transaction transaction, DateOnly date) =>
        accountBalanceHistoryRepository.GetAllHistoriesLaterThan(accountId, date, 0)
            .Count(history => history.Date == date && transactionRepository.GetById(history.TransactionId).Sequence < transaction.Sequence) + 1;

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
        return new AccountBalance(account, account.OnboardedBalance ?? 0);
    }
}