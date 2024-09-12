namespace Domain.Events;

/// <summary>
/// Domain event that indicates that an Accounting Period has been modified
/// </summary>
public class AccountingPeriodChangedEvent : IDomainEvent
{
    /// <summary>
    /// Year of the changed Accounting Period
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Month of the changed Accounting Period
    /// </summary>
    public required int Month { get; init; }

    /// <summary>
    /// Action that was performed on this Accounting Period
    /// </summary>
    public AccountingPeriodChangedAction Action { get; init; }
}

/// <summary>
/// Enum representing the different actions that can be performed on an Accounting Period
/// </summary>
public enum AccountingPeriodChangedAction
{
    /// <summary>
    /// A new Accounting Period was added
    /// </summary>
    Added,

    /// <summary>
    /// An existing Accounting Period was deleted
    /// </summary>
    Deleted,
}