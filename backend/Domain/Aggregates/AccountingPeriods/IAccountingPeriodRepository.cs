using Domain.ValueObjects;

namespace Domain.Aggregates.AccountingPeriods;

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
    /// Finds the Accounting Period with the specified external ID.
    /// </summary>
    /// <param name="id">ID of the Accounting Period to find</param>
    /// <returns>The Accounting Period that was found, or null if one wasn't found</returns>
    AccountingPeriod? FindByExternalIdOrNull(Guid id);

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
    /// Finds the latest Accounting Period with Account Balance Checkpoints that starts before the provided date
    /// </summary>
    /// <remarks>
    /// If the provided date falls within a closed Accounting Period, the latest period is the Accounting 
    /// Period the date falls within. If the provided date falls within an open Accounting Period, the latest 
    /// period is the earliest Accounting Period that is still open.
    /// </remarks>
    /// <param name="asOfDate">Date that the found Accounting Period must start before</param>
    /// <returns>The latest Accounting Period with Account Balance Checkpoints to use for balance calculations</returns>
    AccountingPeriod FindLatestAccountingPeriodWithBalanceCheckpoints(DateOnly asOfDate);

    /// <summary>
    /// Finds the list of Accounting Periods that have Balance Events that fall in the provided Date Range
    /// </summary>
    /// <param name="dateRange">Date Range to find Accounting Periods with Balance Events within</param>
    /// <returns>The list of Accounting Periods that have Balance Events that fall in the provided Date Range</returns>
    IReadOnlyCollection<AccountingPeriod> FindAccountingPeriodsWithBalanceEventsInDateRange(DateRange dateRange);

    /// <summary>
    /// Finds the current maximum Balance Event Sequence number for the provided date
    /// </summary>
    /// <param name="eventDate">Event Date to find the maximum sequence number for</param>
    /// <returns>The maximum Balance Event Sequence number for the provided date</returns>
    int FindMaximumBalanceEventSequenceForDate(DateOnly eventDate);

    /// <summary>
    /// Adds the provided Accounting Period to the repository
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period that should be added</param>
    void Add(AccountingPeriod accountingPeriod);
}