using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Interface representing methods to interact wiht a collection of <see cref="Transaction"/>
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Finds all the Transactions that fall in the Accounting Period that the provided date falls within 
    /// </summary>
    /// <param name="accountingPeriod">Date that falls in an Accounting Period</param>
    /// <returns>The collection of Transactions that fall within the Accounting Period</returns>
    IReadOnlyCollection<Transaction> FindAllByAccountingPeriod(DateOnly accountingPeriod);

    /// <summary>
    /// Finds all the Transactions made against the provided Account
    /// </summary>
    /// <param name="accountId">ID of the Account</param>
    /// <returns>The collection of Transactions that were made against the provided Account</returns>
    IReadOnlyCollection<Transaction> FindAllByAccount(Guid accountId);

    /// <summary>
    /// Finds all the Transactions made against the provided Account over the provided date range
    /// </summary>
    /// <param name="accountId">ID of the Account</param>
    /// <param name="startDate">Start date of the date range</param>
    /// <param name="endDate">End date of the date range (inclusive)</param>
    /// <param name="dateToCompare">The date on the transaction that should be used for the comparison</param>
    /// <returns>The collection of Transactions that were made against the provided Account over the provided date range</returns>
    IReadOnlyCollection<Transaction> FindAllByAccountOverDateRange(Guid accountId,
        DateOnly startDate,
        DateOnly endDate,
        DateToCompare dateToCompare);

    /// <summary>
    /// Adds the provided Transaction to the repository
    /// </summary>
    /// <param name="transaction">Transaction that should be added</param>
    void Add(Transaction transaction);
}

/// <summary>
/// Enum representing which date on the Transaction to use for comparison
/// </summary>
[Flags]
public enum DateToCompare
{
    /// <inheritdoc cref="Transaction.AccountingDate"/>
    Accounting = 1,

    /// <inheritdoc cref="TransactionDetail.StatementDate"/>
    Statement = 2,
}