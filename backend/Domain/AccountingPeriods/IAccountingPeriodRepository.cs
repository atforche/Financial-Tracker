namespace Domain.AccountingPeriods;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="AccountingPeriod"/>
/// </summary>
public interface IAccountingPeriodRepository
{
    /// <summary>
    /// Finds all the Accounting Periods currently in the repository
    /// </summary>
    /// <returns>All the Accounting Periods in the Repository</returns>
    IReadOnlyCollection<AccountingPeriod> FindAll();

    /// <summary>
    /// Determines if an Accounting Period with the provided ID exists
    /// </summary>
    /// <param name="id">ID of the Accounting Period</param>
    /// <returns>True if an Accounting Period with the provided ID exists, false otherwise</returns>
    bool DoesAccountingPeriodWithIdExist(Guid id);

    /// <summary>
    /// Finds the Accounting Period with the specified ID.
    /// </summary>
    /// <param name="id">ID of the Accounting Period to find</param>
    /// <returns>The Accounting Period that was found, or null if one wasn't found</returns>
    AccountingPeriod FindById(AccountingPeriodId id);

    /// <summary>
    /// Finds the Accounting Period that the provided date falls within
    /// </summary>
    /// <param name="asOfDate">Date that corresponds to an Accounting Period</param>
    /// <returns>The Accounting Period that the provided date falls within, or null if one wasn't found</returns>
    AccountingPeriod? FindByDateOrNull(DateOnly asOfDate);

    /// <summary>
    /// Finds the Accounting Periods that are currently open
    /// </summary>
    /// <returns>The list of open Accounting Periods</returns>
    IReadOnlyCollection<AccountingPeriod> FindOpenPeriods();

    /// <summary>
    /// Adds the provided Accounting Period to the repository
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period that should be added</param>
    void Add(AccountingPeriod accountingPeriod);
}