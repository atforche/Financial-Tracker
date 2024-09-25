namespace Domain.Events;

/// <summary>
/// Base class that implements functionality for a class to raise domain events
/// </summary>
public abstract class DomainEventRaiser
{
    private readonly List<IDomainEvent> _domainEvents;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    protected DomainEventRaiser()
    {
        _domainEvents = [];
    }

    /// <summary>
    /// Raise a domain event associated with this entity
    /// </summary>
    /// <param name="domainEvent">Domain event to be raised</param>
    public void RaiseEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Gets the list of domain events associated with this entity
    /// </summary>
    /// <returns>The list of domain events associated with this entity</returns>
    public IEnumerable<IDomainEvent> GetDomainEvents() => _domainEvents;

    /// <summary>
    /// Clears the list of events associated with this entity
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}