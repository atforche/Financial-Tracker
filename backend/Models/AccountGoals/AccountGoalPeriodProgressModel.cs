namespace Models.AccountGoals;

/// <summary>
/// Model pairing a Account Goal with its progress in the goal's Accounting Period.
/// </summary>
public sealed class AccountGoalPeriodProgressModel
{
    /// <summary>
    /// Gets the Account Goal.
    /// </summary>
    public required AccountGoalModel Goal { get; init; }

    /// <summary>
    /// Gets the calculated progress for the Account Goal.
    /// </summary>
    public required AccountGoalProgressModel Progress { get; init; }
}
