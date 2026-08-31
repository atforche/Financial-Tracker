namespace Domain.FundGoals;

/// <summary>
/// Projection comparing a Fund's financial state with its Fund Goal.
/// </summary>
public sealed class FundGoalProgress
{
    /// <summary>
    /// Available-balance health.
    /// </summary>
    public AvailableBalanceProgress AvailableBalance { get; }

    /// <summary>
    /// Contribution progress, or null when no contribution dimension is configured.
    /// </summary>
    public ContributionProgress? Contribution { get; }

    /// <summary>
    /// Ending-balance progress, or null when no ending-balance bounds are configured.
    /// </summary>
    public FundGoalEndingBalanceProgress? EndingBalance { get; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundGoalProgress(
        AvailableBalanceProgress availableBalance,
        ContributionProgress? contribution,
        FundGoalEndingBalanceProgress? endingBalance)
    {
        AvailableBalance = availableBalance;
        Contribution = contribution;
        EndingBalance = endingBalance;
    }
}
