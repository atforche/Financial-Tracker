using MediatR;

namespace Domain.Events;

/// <summary>
/// Interface representing a domain event that can be raised and handled
/// </summary>
public interface IDomainEvent : INotification
{
}