namespace Models.AccountGoals;

/// <summary>
/// Model comparing an Account's financial state with its Account Goal.
/// </summary>
public sealed class AccountGoalProgressModel
{
    /// <summary>
    /// Gets positive-balance health.
    /// </summary>
    public required PositiveBalanceProgressModel PositiveBalance { get; init; }

    /// <summary>
    /// Gets whether the Account Goal is achieved.
    /// </summary>
    public required bool IsSatisfied { get; init; }

    /// <summary>
    /// Gets ending-balance progress when bounds are configured.
    /// </summary>
    public EndingBalanceProgressModel? EndingBalance { get; init; }
}
