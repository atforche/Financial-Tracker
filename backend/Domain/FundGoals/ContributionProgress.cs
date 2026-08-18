namespace Domain.FundGoals;

/// <summary>
/// Projection of contribution progress for a Fund Goal.
/// </summary>
public sealed class ContributionProgress
{
    /// <summary>
    /// Recommended contribution after applying the configured bounds.
    /// </summary>
    public decimal TargetAmount { get; }

    /// <summary>
    /// Amount assigned during the Accounting Period.
    /// </summary>
    public decimal AssignedAmount { get; }

    /// <summary>
    /// Nonnegative amount remaining to reach the recommendation.
    /// </summary>
    public decimal RemainingAmount => Math.Max(TargetAmount - AssignedAmount, 0);

    /// <summary>
    /// True when the assigned amount reaches the recommendation.
    /// </summary>
    public bool IsSatisfied => AssignedAmount >= TargetAmount;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal ContributionProgress(decimal targetAmount, decimal assignedAmount)
    {
        TargetAmount = targetAmount;
        AssignedAmount = assignedAmount;
    }
}
