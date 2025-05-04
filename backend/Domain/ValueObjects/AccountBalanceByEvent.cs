using Domain.Aggregates;

namespace Domain.ValueObjects;

/// <summary>
/// Value object class representing an Account Balance associated with a Balance Event
/// </summary>
public record AccountBalanceByEvent
{
    /// <summary>
    /// Balance Event for this Account Balance by Event
    /// </summary>
    public required BalanceEvent BalanceEvent { get; init; }

    /// <summary>
    /// Account Balance for this Account Balance by Event
    /// </summary>
    public required AccountBalance AccountBalance { get; init; }
}