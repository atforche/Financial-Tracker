using Domain.Entities;

namespace Domain.Repositories;

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
    /// Finds the Accounting Period with the specified Id.
    /// </summary>
    /// <param name="id">Id of the Accounting Period to find</param>
    /// <returns>The Accounting Period that was found, or null if one wasn't found</returns>
    AccountingPeriod? FindOrNull(Guid id);

    /// <summary>
    /// Finds the Accounting Period that corresponds with the provided date
    /// </summary>
    /// <param name="asOfDate">Date that corresponds to an Accounting Period</param>
    /// <returns>The Accounting Period that corresponds with the provided date</returns>
    AccountingPeriod? FindOrNullByDate(DateOnly asOfDate);

    /// <summary>
    /// Finds the Accounting Periods that are currently open
    /// </summary>
    /// <returns>The open Accounting Periods</returns>
    ICollection<AccountingPeriod> FindOpenPeriods();

    /// <summary>
    /// Finds the effective Accounting Period to use for balance calculations as of the provided date. If the provided
    /// date falls within a closed Accounting Period, the effective period is the Accounting Period the date falls 
    /// within. If the provided date falls within an open Accounting Period, the effective period is the earliest 
    /// Accounting Period that is still open.
    /// </summary>
    /// <param name="asOfDate">Date to find the effective balance period as of</param>
    /// <returns>The effective Accounting Period to use for balance calculations</returns>
    AccountingPeriod FindEffectiveAccountingPeriodForBalances(DateOnly asOfDate);

    /// <summary>
    /// Adds the provided Accounting Period to the repository
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period that should be added</param>
    void Add(AccountingPeriod accountingPeriod);

    /// <summary>
    /// Updates the provided Accounting Period in the repository
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to update in the repository</param>
    void Update(AccountingPeriod accountingPeriod);

    /// <summary>
    /// Deletes the Accounting Period with the specified id
    /// </summary>
    /// <param name="id">Id of the Accounting Period to delete</param>
    void Delete(Guid id);
}