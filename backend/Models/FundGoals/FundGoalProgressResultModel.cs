namespace Models.FundGoals;

/// <summary>
/// Model pairing a Fund Goal with its progress for an Accounting Period.
/// </summary>
public sealed class FundGoalProgressResultModel
{
    /// <summary>
    /// Gets the ID of the Fund Goal.
    /// </summary>
    public required Guid FundGoalId { get; init; }

    /// <summary>
    /// Gets the calculated progress for the Fund Goal.
    /// </summary>
    public required FundGoalProgressModel Progress { get; init; }
}