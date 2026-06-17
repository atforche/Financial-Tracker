namespace Models.Goals;

/// <summary>
/// Model representing a recent balance event for a current Goal progress section.
/// </summary>
public class CurrentGoalBalanceEventModel
{
    /// <summary>
    /// Transaction that produced this balance event.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Effective date of the balance event.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Whether the transaction has been posted to the fund.
    /// </summary>
    public required bool IsPosted { get; init; }

    /// <summary>
    /// Amount associated with the balance event.
    /// </summary>
    public required decimal Amount { get; init; }
}