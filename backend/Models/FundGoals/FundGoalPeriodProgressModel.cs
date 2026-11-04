namespace Models.FundGoals;

/// <summary>
/// Model pairing a Fund Goal with its progress in the goal's Accounting Period.
/// </summary>
public sealed class FundGoalPeriodProgressModel
{
    /// <summary>
    /// Gets the Fund Goal.
    /// </summary>
    public required FundGoalModel Goal { get; init; }

    /// <summary>
    /// Gets the calculated progress for the Fund Goal.
    /// </summary>
    public required FundGoalProgressModel Progress { get; init; }
}
