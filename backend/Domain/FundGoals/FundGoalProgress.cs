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
    /// Funded-balance progress, or null when no funded-balance bounds are configured.
    /// </summary>
    public FundedBalanceProgress? FundedBalance { get; }

    /// <summary>
    /// Ending-balance progress, or null when no target ending balance is configured.
    /// </summary>
    public EndingBalanceProgress? EndingBalance { get; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundGoalProgress(
        AvailableBalanceProgress availableBalance,
        ContributionProgress? contribution,
        FundedBalanceProgress? fundedBalance,
        EndingBalanceProgress? endingBalance)
    {
        AvailableBalance = availableBalance;
        Contribution = contribution;
        FundedBalance = fundedBalance;
        EndingBalance = endingBalance;
    }
}