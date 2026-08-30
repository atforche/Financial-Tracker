namespace Domain.AccountGoals;

/// <summary>
/// Record representing an Account Goal configuration update.
/// </summary>
public sealed record UpdateAccountGoalRequest
{
    /// <summary>
    /// Minimum desired ending balance.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Maximum desired ending balance.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}
