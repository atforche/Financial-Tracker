namespace Models.AccountGoals;

/// <summary>
/// Model pairing an Account Goal with its progress for an Accounting Period.
/// </summary>
public sealed class AccountGoalProgressResultModel
{
    /// <summary>
    /// Gets the ID of the Account Goal.
    /// </summary>
    public required Guid AccountGoalId { get; init; }

    /// <summary>
    /// Gets the calculated progress for the Account Goal.
    /// </summary>
    public required AccountGoalProgressModel Progress { get; init; }
}
