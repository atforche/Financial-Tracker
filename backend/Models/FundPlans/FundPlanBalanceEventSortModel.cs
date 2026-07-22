namespace Models.FundPlans;

/// <summary>
/// Available ordering options for Fund Plan balance events.
/// </summary>
public enum FundPlanBalanceEventSortModel
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
}