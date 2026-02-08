using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="AccountBalanceHistory"/>
/// </summary>
public interface IAccountBalanceHistoryRepository
{
    /// <summary>
    /// Gets the next sequence number for the specified Account ID and date
    /// </summary>
    int GetNextSequenceForAccountAndDate(AccountId accountId, DateOnly historyDate);

    /// <summary>
    /// Gets the latest Account Balance History entry for the specified Account ID
    /// </summary>
    AccountBalanceHistory FindLatestForAccount(AccountId accountId);

    /// <summary>
    /// Gets all Account Balance History entries for the specified Transaction ID
    /// </summary>
    IReadOnlyCollection<AccountBalanceHistory> GetAllByTransactionId(TransactionId transactionId);

    /// <summary>
    /// Finds the earliest Account Balance History entry for the specified Transaction ID
    /// </summary>
    AccountBalanceHistory FindEarliestByTransactionId(AccountId accountId, TransactionId transactionId);

    /// <summary>
    /// Finds the latest Account Balance History entry earlier than the specified date and sequence number
    /// </summary>
    AccountBalanceHistory? FindLatestHistoryEarlierThan(AccountId accountId, DateOnly historyDate, int sequenceNumber);

    /// <summary>
    /// Gets all Account Balance History entries later than the specified date and sequence number
    /// </summary>
    IReadOnlyCollection<(AccountBalanceHistory History, Transaction Transaction)> FindAllHistoriesLaterThan(AccountId accountId, DateOnly historyDate, int sequence);

    /// <summary>
    /// Adds the provided Account Balance History to the repository
    /// </summary>
    void Add(AccountBalanceHistory accountBalanceHistory);

    /// <summary>
    /// Deletes the provided Account Balance History from the repository
    /// </summary>
    void Delete(AccountBalanceHistory accountBalanceHistory);
}