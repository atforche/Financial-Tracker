namespace Models.FundGoals;

/// <summary>
/// Model representing a named source or destination for a Fund Goal balance event.
/// </summary>
public sealed class FundGoalBalanceEventPartyModel
{
    /// <summary>
    /// Display name for the source or destination.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Amount associated with the source or destination when applicable.
    /// </summary>
    public decimal? Amount { get; init; }
}
