namespace Domain.AccountingPeriods;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="AccountingPeriod"/>
/// </summary>
public interface IAccountingPeriodRepository
{
    /// <summary>
    /// Gets the Accounting Period with the specified ID.
    /// </summary>
    AccountingPeriod GetById(AccountingPeriodId id);

    /// <summary>
    /// Gets the Accounting Period with the specified year and month, or null if it doesn't exist.
    /// </summary>
    AccountingPeriod? GetByYearAndMonth(int year, int month);

    /// <summary>
    /// Gets the latest Accounting Period
    /// </summary>
    AccountingPeriod? GetLatestAccountingPeriod();

    /// <summary>
    /// Gets the next Accounting Period for the Accounting Period with the specified ID
    /// </summary>
    AccountingPeriod? GetNextAccountingPeriod(AccountingPeriodId id);

    /// <summary>
    /// Gets all the Accounting Periods that are currently open
    /// </summary>
    IReadOnlyCollection<AccountingPeriod> GetAllOpenPeriods();

    /// <summary>
    /// Adds the provided Accounting Period to the repository
    /// </summary>
    void Add(AccountingPeriod accountingPeriod);

    /// <summary>
    /// Deletes the provided Accounting Period from the repository
    /// </summary>
    void Delete(AccountingPeriod accountingPeriod);
}