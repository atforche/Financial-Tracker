using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="FundBalanceHistory"/>
/// </summary>
public interface IFundBalanceHistoryRepository
{
    /// <summary>
    /// Gets the next sequence number for the specified Fund ID and date
    /// </summary>
    int GetNextSequenceForFundAndDate(FundId fundId, DateOnly historyDate);

    /// <summary>
    /// Gets the latest Fund Balance History entry for the specified Fund ID
    /// </summary>
    FundBalanceHistory? GetLatestForFund(FundId fundId);

    /// <summary>
    /// Gets all Fund Balance History entries for the specified Transaction ID
    /// </summary>
    IReadOnlyCollection<FundBalanceHistory> GetAllByTransactionId(TransactionId transactionId);

    /// <summary>
    /// Gets the earliest Fund Balance History entry for the specified Transaction ID
    /// </summary>
    FundBalanceHistory GetEarliestByTransactionId(FundId fundId, TransactionId transactionId);

    /// <summary>
    /// Gets the latest Fund Balance History entry earlier than the specified date and sequence number
    /// </summary>
    FundBalanceHistory? GetLatestHistoryEarlierThan(FundId fundId, DateOnly historyDate, int sequenceNumber);

    /// <summary>
    /// Gets all Fund Balance History entries later than the specified date and sequence number
    /// </summary>
    IReadOnlyCollection<(FundBalanceHistory History, Transaction Transaction)> GetAllHistoriesLaterThan(FundId fundId, DateOnly historyDate, int sequence);

    /// <summary>
    /// Adds the provided Fund Balance History to the repository
    /// </summary>
    void Add(FundBalanceHistory fundBalanceHistory);

    /// <summary>
    /// Deletes the provided Fund Balance History from the repository
    /// </summary>
    void Delete(FundBalanceHistory fundBalanceHistory);
}