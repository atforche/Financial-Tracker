using Domain.AccountingPeriods;

namespace Domain.ChangeInValues;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="ChangeInValue"/>
/// </summary>
public interface IChangeInValueRepository
{
    /// <summary>
    /// Determines if a Change In Value with the provided ID exists
    /// </summary>
    /// <param name="id">ID of the Change In Value</param>
    /// <returns>True if a Change In Value with the provided ID exists, false otherwise</returns>
    bool DoesChangeInValueWithIdExist(Guid id);

    /// <summary>
    /// Finds the Change In Value with the specified ID.
    /// </summary>
    /// <param name="id">ID of the Change In Value to find</param>
    /// <returns>The Change In Value that was found</returns>
    ChangeInValue FindById(ChangeInValueId id);

    /// <summary>
    /// Finds all the Change In Values with the specified Accounting Period ID
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID</param>
    /// <returns>All the Change In Values with the specified Accounting Period ID</returns>
    IReadOnlyCollection<ChangeInValue> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Finds all the Change In Values that were added in the specified Date Range
    /// </summary>
    /// <param name="dateRange">Date Range</param>
    /// <returns>All the Change In Values that were added in the specified Date Range</returns>
    IReadOnlyCollection<ChangeInValue> FindAllByDateRange(DateRange dateRange);

    /// <summary>
    /// Adds the provided Change In Value to the repository
    /// </summary>
    /// <param name="changeInValue">Change In Value that should be added</param>
    void Add(ChangeInValue changeInValue);
}