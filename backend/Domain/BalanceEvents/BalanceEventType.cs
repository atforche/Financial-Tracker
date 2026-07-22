namespace Domain.BalanceEvents;

/// <summary>
/// Identifies whether a balance event debits or credits a balance.
/// </summary>
public enum BalanceEventType
{
    /// <summary>
    /// Debit event.
    /// </summary>
    Debit,

    /// <summary>
    /// Credit event.
    /// </summary>
    Credit,
}