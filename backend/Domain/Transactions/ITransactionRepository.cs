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
    /// Gets the next sequence number for the specified transaction date
    /// </summary>
    int GetNextSequenceForDate(DateOnly transactionDate);

    /// <summary>
    /// Gets all the Transactions that are associated with the specified Account
    /// </summary>
    IReadOnlyCollection<Transaction> GetAllByAccount(AccountId accountId);

    /// <summary>
    /// Gets all the Transactions that are associated with the specified Accounting Period
    /// </summary>
    IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Gets all the Transactions that are associated with the specified Fund
    /// </summary>
    IReadOnlyCollection<Transaction> GetAllByFund(FundId fundId);

    /// <summary>
    /// Gets the Transaction with the specified ID.
    /// </summary>
    Transaction GetById(TransactionId id);

    /// <summary>
    /// Attempts to get the Transaction with the specified ID
    /// </summary>
    bool TryGetById(Guid id, [NotNullWhen(true)] out Transaction? transaction);

    /// <summary>
    /// Adds the provided Transaction to the repository
    /// </summary>
    void Add(Transaction transaction);

    /// <summary>
    /// Deletes the provided Transaction from the repository
    /// </summary>
    void Delete(Transaction transaction);
}