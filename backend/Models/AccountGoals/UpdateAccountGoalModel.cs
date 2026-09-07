namespace Models.AccountGoals;

/// <summary>
/// Model representing Account Goal configuration updates.
/// </summary>
public sealed class UpdateAccountGoalModel
{
    /// <summary>
    /// Gets the minimum desired ending balance. Omitted or null values default to zero.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Gets the maximum desired ending balance.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}
