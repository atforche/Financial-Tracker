namespace Domain.BalanceEvents;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="BalanceEvent"/>
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
}