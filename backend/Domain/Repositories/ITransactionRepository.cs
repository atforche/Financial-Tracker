using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Interface representing methods to interact wiht a collection of <see cref="Transaction"/>
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Finds all the Transactions that fall within the specified Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Date representing the Accounting Period</param>
    /// <returns>The collection of Transactions that fall within the specified Accounting Period</returns>
    IReadOnlyCollection<Transaction> FindAllByAccountingPeriod(DateOnly accountingPeriod);

    /// <summary>
    /// Finds all the Transactions made against the provided Account
    /// </summary>
    /// <param name="accountId">ID of the Account</param>
    /// <returns>The collection of Transactions that were made against the provided Account</returns>
    IReadOnlyCollection<Transaction> FindAllByAccount(Guid accountId);

    /// <summary>
    /// Finds the Transaction with the specified ID
    /// </summary>
    /// <param name="id">ID of the Transaction to find</param>
    /// <returns>The Transaction that was found, or null if one wasn't found</returns>
    Transaction? FindOrNull(Guid id);

    /// <summary>
    /// Adds the provided Transaction to the repository
    /// </summary>
    /// <param name="transaction">Transaction that should be added</param>
    void Add(Transaction transaction);

    /// <summary>
    /// Updates the provided Transaction in the repository
    /// </summary>
    /// <param name="transaction">Transaction to update in the repository</param>
    void Update(Transaction transaction);

    /// <summary>
    /// Deletes the Transaction with the specified ID
    /// </summary>
    /// <param name="id">ID of the Transaction to delete</param>
    void Delete(Guid id);
}