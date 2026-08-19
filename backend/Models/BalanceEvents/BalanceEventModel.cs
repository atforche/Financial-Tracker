using Models.AccountingPeriods;

namespace Models.BalanceEvents;

/// <summary>
/// Model representing a balance event.
/// </summary>
public class BalanceEventModel
{
    /// <summary>
    /// Accounting Period for the balance event.
    /// </summary>
    public required AccountingPeriodModel AccountingPeriod { get; init; }

    /// <summary>
    /// Transaction for the balance event.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Description for the transaction associated with the balance event.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Date on which the Transaction was created.
    /// </summary>
    public required DateOnly TransactionDate { get; init; }

    /// <summary>
    /// Sequence of the Transaction on its transaction date.
    /// </summary>
    public required int TransactionSequence { get; init; }

    /// <summary>
    /// Event date for the balance event, or null if it's still pending.
    /// </summary>
    public DateOnly? EventDate { get; init; }

    /// <summary>
    /// Sequence within the event date, or null when the event is pending.
    /// </summary>
    public int? EventDateSequence { get; init; }

    /// <summary>
    /// Type of balance event.
    /// </summary>
    public required BalanceEventTypeModel Type { get; init; }

    /// <summary>
    /// True if the balance event has been posted, false otherwise.
    /// </summary>
    public required bool IsPosted { get; init; }

    /// <summary>
    /// Amount associated with the balance event.
    /// </summary>
    public required decimal Amount { get; init; }
}
