namespace Domain.FundGoals;

/// <summary>
/// Projection comparing a Fund's financial state with its Fund Goal.
/// </summary>
public sealed class FundGoalProgress
{
    /// <summary>
    /// Contribution progress, or null when no contribution dimension is configured.
    /// </summary>
    public ContributionProgress? Contribution { get; }

    /// <summary>
    /// Ending-balance progress, with a default minimum of zero.
    /// </summary>
    public FundGoalEndingBalanceProgress EndingBalance { get; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundGoalProgress(
        ContributionProgress? contribution,
        FundGoalEndingBalanceProgress endingBalance)
    {
        Contribution = contribution;
        EndingBalance = endingBalance;
    }
}
