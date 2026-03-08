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
        accountBalanceHistoryRepository.GetLatestForAccount(account.Id)?.ToAccountBalance() ?? new AccountBalance(account, [], [], []);

    /// <summary>
    /// Gets the Account Balance prior to the provided Transaction
    /// </summary>
    public AccountBalance GetPreviousBalanceForTransaction(TransactionAccount transactionAccount)
    {
        var balanceHistories = accountBalanceHistoryRepository.GetAllByTransactionId(transactionAccount.Transaction.Id).ToList();
        if (transactionAccount.PostedDate != null)
        {
            AccountBalanceHistory postedHistory = balanceHistories.First(bh => bh.Date == transactionAccount.PostedDate);
            return GetExistingAccountBalanceAsOf(
                accountRepository.GetById(transactionAccount.AccountId),
                postedHistory.Date,
                postedHistory.Sequence);
        }
        AccountBalanceHistory earliestHistory = balanceHistories.OrderBy(bh => bh.Date).ThenBy(bh => bh.Sequence).First();
        return GetExistingAccountBalanceAsOf(
            accountRepository.GetById(transactionAccount.AccountId),
            earliestHistory.Date,
            earliestHistory.Sequence);
    }

    /// <summary>
    /// Gets the Account Balance after the provided Transaction
    /// </summary>
    public AccountBalance GetNewBalanceForTransaction(TransactionAccount transactionAccount)
    {
        var balanceHistories = accountBalanceHistoryRepository.GetAllByTransactionId(transactionAccount.Transaction.Id).ToList();
        if (transactionAccount.PostedDate != null)
        {
            return balanceHistories.Single(bh => bh.Date == transactionAccount.PostedDate).ToAccountBalance();
        }
        return balanceHistories.OrderBy(bh => bh.Date).ThenBy(bh => bh.Sequence).First().ToAccountBalance();
    }

    /// <summary>
    /// Updates the Account Balances for a newly added Transaction
    /// </summary>
    internal void AddTransaction(Transaction newTransaction)
    {
        if (newTransaction.DebitAccount != null && newTransaction.CreditAccount != null && newTransaction.DebitAccount.AccountId == newTransaction.CreditAccount.AccountId)
        {
            AddNewBalanceHistory(newTransaction, newTransaction.DebitAccount, newTransaction.Date);
            return;
        }
        if (newTransaction.DebitAccount != null)
        {
            AddNewBalanceHistory(newTransaction, newTransaction.DebitAccount, newTransaction.Date);
        }
        if (newTransaction.CreditAccount != null)
        {
            AddNewBalanceHistory(newTransaction, newTransaction.CreditAccount, newTransaction.Date);
        }
    }

    /// <summary>
    /// Updates the Account Balances for an updated Transaction
    /// </summary>
    internal void UpdateTransaction(Transaction transaction)
    {
        if (transaction.DebitAccount != null && transaction.CreditAccount != null && transaction.DebitAccount.AccountId == transaction.CreditAccount.AccountId)
        {
            UpdateExistingBalanceHistory(transaction, transaction.DebitAccount);
            return;
        }
        if (transaction.DebitAccount != null)
        {
            UpdateExistingBalanceHistory(transaction, transaction.DebitAccount);
        }
        if (transaction.CreditAccount != null)
        {
            UpdateExistingBalanceHistory(transaction, transaction.CreditAccount);
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
            UpdateExistingBalanceHistory(transaction, transactionAccount);
        }
        else
        {
            AddNewBalanceHistory(transaction, transactionAccount, transactionAccount.PostedDate.Value);
        }
    }

    /// <summary>
    /// Updates the Account Balances for a deleted Transaction
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (AccountBalanceHistory balanceHistory in accountBalanceHistoryRepository.GetAllByTransactionId(transaction.Id))
        {
            DeleteExistingBalanceHistory(balanceHistory);
            accountBalanceHistoryRepository.Delete(balanceHistory);
        }
    }

    /// <summary>
    /// Adds a new Account Balance History entry
    /// </summary>
    private void AddNewBalanceHistory(Transaction transaction, TransactionAccount transactionAccount, DateOnly date)
    {
        int sequence = accountBalanceHistoryRepository.GetNextSequenceForAccountAndDate(transactionAccount.AccountId, date);
        AccountBalance existingBalance = GetExistingAccountBalanceAsOf(
            accountRepository.GetById(transactionAccount.AccountId),
            date,
            sequence);
        AccountBalance newBalance = transaction.ApplyToAccountBalance(existingBalance, date);
        var newBalanceHistory = new AccountBalanceHistory(newBalance.Account,
            transaction.Id,
            date,
            sequence,
            newBalance.FundBalances,
            newBalance.PendingDebits,
            newBalance.PendingCredits);

        foreach ((AccountBalanceHistory history, Transaction existingTransaction) in accountBalanceHistoryRepository
            .GetAllHistoriesLaterThan(newBalanceHistory.Account.Id, newBalanceHistory.Date, newBalanceHistory.Sequence))
        {
            if (history.Date == newBalanceHistory.Date)
            {
                history.Sequence += 1;
            }
            AccountBalance updatedBalance = existingTransaction.ApplyToAccountBalance(newBalance, history.Date);
            history.FundBalances = updatedBalance.FundBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            newBalance = updatedBalance;
        }
        accountBalanceHistoryRepository.Add(newBalanceHistory);
    }

    /// <summary>
    /// Updates an existing Account Balance History entry
    /// </summary>
    private void UpdateExistingBalanceHistory(Transaction transaction, TransactionAccount transactionAccount)
    {
        AccountBalanceHistory existingHistory = accountBalanceHistoryRepository.GetEarliestByTransactionId(transactionAccount.AccountId, transaction.Id);
        AccountBalance existingBalance = GetExistingAccountBalanceAsOf(
            accountRepository.GetById(transactionAccount.AccountId),
            existingHistory.Date,
            existingHistory.Sequence);
        AccountBalance newBalance = transaction.ApplyToAccountBalance(existingBalance, existingHistory.Date);
        existingHistory.FundBalances = newBalance.FundBalances;
        existingHistory.PendingDebits = newBalance.PendingDebits;
        existingHistory.PendingCredits = newBalance.PendingCredits;

        foreach ((AccountBalanceHistory history, Transaction existingTransaction) in accountBalanceHistoryRepository
            .GetAllHistoriesLaterThan(existingHistory.Account.Id, existingHistory.Date, existingHistory.Sequence))
        {
            AccountBalance updatedBalance = existingTransaction.ApplyToAccountBalance(newBalance, history.Date);
            history.FundBalances = updatedBalance.FundBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            newBalance = updatedBalance;
        }
    }

    /// <summary>
    /// Deletes an existing Account Balance History entry
    /// </summary>
    private void DeleteExistingBalanceHistory(AccountBalanceHistory deletedBalanceHistory)
    {
        AccountBalance existingBalance = GetExistingAccountBalanceAsOf(deletedBalanceHistory.Account, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence);
        foreach ((AccountBalanceHistory history, Transaction transaction) in accountBalanceHistoryRepository
            .GetAllHistoriesLaterThan(deletedBalanceHistory.Account.Id, deletedBalanceHistory.Date, deletedBalanceHistory.Sequence + 1))
        {
            if (history.Date == deletedBalanceHistory.Date)
            {
                history.Sequence -= 1;
            }
            AccountBalance updatedBalance = transaction.ApplyToAccountBalance(existingBalance, history.Date);
            history.FundBalances = updatedBalance.FundBalances;
            history.PendingDebits = updatedBalance.PendingDebits;
            history.PendingCredits = updatedBalance.PendingCredits;
            existingBalance = updatedBalance;
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
        return new AccountBalance(account, [], [], []);
    }
}