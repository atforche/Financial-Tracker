namespace Models.FundGoals;

/// <summary>
/// Available ordering options for Fund Goal balance events.
/// </summary>
public enum FundGoalBalanceEventSortModel
{
    /// <summary>
    /// Sorts by Fund name.
    /// </summary>
    FundName,

    /// <summary>
    /// Sorts by Fund name descending.
    /// </summary>
    FundNameDescending,

    /// <summary>
    /// Sorts by date.
    /// </summary>
    Date,

    /// <summary>
    /// Sorts by date descending.
    /// </summary>
    DateDescending,

    /// <summary>
    /// Sorts by event type.
    /// </summary>
    Type,

    /// <summary>
    /// Sorts by event type descending.
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Sorts by amount.
    /// </summary>
    Amount,

    /// <summary>
    /// Sorts by amount descending.
    /// </summary>
    AmountDescending,

    /// <summary>
    /// Sorts by the other party for the balance-event direction.
    /// </summary>
    Counterparty,

    /// <summary>
    /// Sorts by the other party for the balance-event direction descending.
    /// </summary>
    CounterpartyDescending,

    /// <summary>
    /// Sorts by source.
    /// </summary>
    Source,

    /// <summary>
    /// Sorts by source descending.
    /// </summary>
    SourceDescending,

    /// <summary>
    /// Sorts by destination.
    /// </summary>
    Destination,

    /// <summary>
    /// Sorts by destination descending.
    /// </summary>
    DestinationDescending,
}
