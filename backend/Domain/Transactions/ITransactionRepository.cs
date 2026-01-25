using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Transaction"/>
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Finds all the Transactions that are associated with the specified Account
    /// </summary>
    IReadOnlyCollection<Transaction> FindAllByAccount(AccountId accountId);

    /// <summary>
    /// Finds all the Transactions that are associated with the specified Accounting Period
    /// </summary>
    IReadOnlyCollection<Transaction> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Finds all the Transactions that are associated with the specified Fund
    /// </summary>
    IReadOnlyCollection<Transaction> FindAllByFund(FundId fundId);

    /// <summary>
    /// Finds the Transaction with the specified ID.
    /// </summary>
    /// <param name="id">ID of the Transaction to find</param>
    /// <returns>The Transaction that was found</returns>
    Transaction FindById(TransactionId id);

    /// <summary>
    /// Attempts to find the Transaction with the specified ID
    /// </summary>
    /// <param name="id">ID of the Transaction to find</param>
    /// <param name="transaction">The Transaction that was found, or null if one wasn't found</param>
    /// <returns>True if a Transaction with the provided ID was found, false otherwise</returns>
    bool TryFindById(Guid id, [NotNullWhen(true)] out Transaction? transaction);

    /// <summary>
    /// Adds the provided Transaction to the repository
    /// </summary>
    /// <param name="transaction">Transaction that should be added</param>
    void Add(Transaction transaction);

    /// <summary>
    /// Deletes the provided Transaction from the repository
    /// </summary>
    /// <param name="transaction">Transaction to be deleted</param>
    void Delete(Transaction transaction);
}