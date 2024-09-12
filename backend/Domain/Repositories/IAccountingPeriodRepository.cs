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
    /// Finds the Accounting Period with the specified id. An error is thrown if no Accounting Period is found.
    /// </summary>
    /// <param name="id">Id of the Accounting Period to find</param>
    /// <returns>The Accounting Period that was found</returns>
    AccountingPeriod Find(Guid id);

    /// <summary>
    /// Finds the Accounting Period with the specified Id.
    /// </summary>
    /// <param name="id">Id of the Accounting Period to find</param>
    /// <returns>The Accounting Period that was found, or null if one wasn't found</returns>
    AccountingPeriod? FindOrNull(Guid id);

    /// <summary>
    /// Finds the Accounting Period that is currently open
    /// </summary>
    /// <returns>The open Accounting Period, or null if none are open</returns>
    AccountingPeriod? FindOpenPeriod();

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