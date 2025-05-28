using Domain.AccountingPeriods;

namespace Domain.Transactions;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Transaction"/>
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Determines if an Transaction with the provided ID exists
    /// </summary>
    /// <param name="id">ID of the Transaction</param>
    /// <returns>True if a Transaction with the provided ID exists, false otherwise</returns>
    bool DoesTransactionWithIdExist(Guid id);

    /// <summary>
    /// Finds the Transaction with the specified ID.
    /// </summary>
    /// <param name="id">ID of the Transaction to find</param>
    /// <returns>The Transaction that was found</returns>
    Transaction FindById(TransactionId id);

    /// <summary>
    /// Finds all the Transactions with the specified Accounting Period ID
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID</param>
    /// <returns>All the Transactions with the specified Accounting Period ID</returns>
    IReadOnlyCollection<Transaction> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Finds all the Transactions that were added or that had balance events in the specified Date Range
    /// </summary>
    /// <param name="dateRange">Date Range</param>
    /// <returns>All the Transactions that were added or that had balance events in the specified Date Range</returns>
    IReadOnlyCollection<Transaction> FindAllByDateRange(DateRange dateRange);

    /// <summary>
    /// Adds the provided Transaction to the repository
    /// </summary>
    /// <param name="transaction">Transaction that should be added</param>
    void Add(Transaction transaction);
}