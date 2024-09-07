using Domain.Events;
using MediatR;

namespace Domain.EventHandlers;

/// <summary>
/// Interface representing an event handler that can respond to domain events
/// </summary>
public interface IDomainEventHandler<T> : INotificationHandler<T> where T : IDomainEvent
{
}