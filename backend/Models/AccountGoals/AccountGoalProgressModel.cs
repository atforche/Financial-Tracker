namespace Models.AccountGoals;

/// <summary>
/// Model comparing an Account's financial state with its Account Goal.
/// </summary>
public sealed class AccountGoalProgressModel
{
    /// <summary>
    /// Gets whether the Account Goal is achieved.
    /// </summary>
    public required bool IsSatisfied { get; init; }

    /// <summary>
    /// Gets ending-balance progress, with a default minimum of zero.
    /// </summary>
    public required AccountGoalEndingBalanceProgressModel EndingBalance { get; init; }
}
