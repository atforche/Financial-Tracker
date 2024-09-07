namespace Domain.Events;

/// <summary>
/// Domain event that indicates that an Account has been given a new name
/// </summary>
public class AccountRenamedDomainEvent : IDomainEvent
{
    /// <summary>
    /// New name for the Account
    /// </summary>
    public required string NewName { get; init; }
}