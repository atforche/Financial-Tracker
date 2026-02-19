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
    /// Checks if any Transactions exist for the specified Account (excluding the initial balance transaction)
    /// </summary>
    bool DoAnyTransactionsExistForAccount(Account account);

    /// <summary>
    /// Gets all the Transactions that are associated with the specified Accounting Period
    /// </summary>
    IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Checks if any Transactions exist for the specified Fund
    /// </summary>
    bool DoAnyTransactionsExistForFund(FundId fundId);

    /// <summary>
    /// Gets the Transaction with the specified ID.
    /// </summary>
    Transaction GetById(TransactionId id);

    /// <summary>
    /// Adds the provided Transaction to the repository
    /// </summary>
    void Add(Transaction transaction);

    /// <summary>
    /// Deletes the provided Transaction from the repository
    /// </summary>
    void Delete(Transaction transaction);
}