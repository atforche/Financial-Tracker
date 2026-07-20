namespace Domain.FundPlans;

/// <summary>
/// Projection comparing a Fund's financial state with its Fund Plan.
/// </summary>
public sealed class FundPlanProgress
{
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
    internal FundPlanProgress(
        ContributionProgress? contribution,
        FundedBalanceProgress? fundedBalance,
        EndingBalanceProgress? endingBalance)
    {
        Contribution = contribution;
        FundedBalance = fundedBalance;
        EndingBalance = endingBalance;
    }
}