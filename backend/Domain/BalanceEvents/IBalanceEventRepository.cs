using Domain.AccountingPeriods;

namespace Domain.BalanceEvents;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="IBalanceEvent"/>
/// </summary>
public interface IBalanceEventRepository
{
    /// <summary>
    /// Gets the current highest Event Sequence for a Balance Event on the provided date
    /// </summary>
    /// <param name="eventDate">Event Date</param>
    /// <returns>
    /// The current highest Event Sequence for a Balance Event on the provided date, 
    /// or zero if no Balance Events exist on the provided date
    /// </returns>
    int GetHighestEventSequenceOnDate(DateOnly eventDate);

    /// <summary>
    /// Finds all the Balance Events for the provided Accounting Period ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <returns>All the Balance Events for the provided Accounting Period ID</returns>
    IReadOnlyCollection<IBalanceEvent> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Finds all the Balance Events for the provided date
    /// </summary>
    /// <param name="eventDate">Event Date</param>
    /// <returns>All the Balance Events for the provided Date</returns>
    IReadOnlyCollection<IBalanceEvent> FindAllByDate(DateOnly eventDate);

    /// <summary>
    /// Finds all the Balance Events for the provided Date Range
    /// </summary>
    /// <param name="dateRange">Date Range</param>
    /// <returns>All the Balance Events for the provided Date Range</returns>
    IReadOnlyCollection<IBalanceEvent> FindAllByDateRange(DateRange dateRange);
}