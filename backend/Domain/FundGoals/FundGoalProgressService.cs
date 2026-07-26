namespace Domain.FundGoals;

/// <summary>
/// Service for calculating Fund Goal progress from financial facts.
/// </summary>
public static class FundGoalProgressService
{
    /// <summary>
    /// Calculates progress for a Fund Goal in an Accounting Period.
    /// </summary>
    public static FundGoalProgress Calculate(
        decimal openingAvailableBalance,
        decimal assignedAmount,
        decimal currentAvailableBalance,
        decimal? regularContribution,
        decimal? minimumFundedBalance,
        decimal? maximumFundedBalance,
        decimal? targetEndingBalance)
    {
        decimal recommendedContribution = CalculateRecommendedContribution(
            openingAvailableBalance,
            regularContribution,
            minimumFundedBalance,
            maximumFundedBalance);
        ContributionProgress? contribution = regularContribution != null
            || minimumFundedBalance != null
            || maximumFundedBalance != null
            ? new ContributionProgress(recommendedContribution, assignedAmount)
            : null;
        FundedBalanceProgress? fundedBalance = minimumFundedBalance != null
            || maximumFundedBalance != null
            ? new FundedBalanceProgress(
                openingAvailableBalance + assignedAmount,
                minimumFundedBalance,
                maximumFundedBalance)
            : null;
        EndingBalanceProgress? endingBalance = targetEndingBalance is decimal targetBalance
            ? new EndingBalanceProgress(targetBalance, currentAvailableBalance)
            : null;

        return new FundGoalProgress(
            new AvailableBalanceProgress(currentAvailableBalance),
            contribution,
            fundedBalance,
            endingBalance);
    }

    /// <summary>
    /// Calculates the recommended contribution after applying funded-balance constraints.
    /// </summary>
    public static decimal CalculateRecommendedContribution(
        decimal openingAvailableBalance,
        decimal? regularContribution,
        decimal? minimumFundedBalance,
        decimal? maximumFundedBalance)
    {
        decimal recommendedBalance = openingAvailableBalance + (regularContribution ?? 0);

        if (minimumFundedBalance is decimal minimum)
        {
            recommendedBalance = Math.Max(recommendedBalance, minimum);
        }
        if (maximumFundedBalance is decimal maximum)
        {
            recommendedBalance = Math.Min(recommendedBalance, maximum);
        }
        return Math.Max(recommendedBalance - openingAvailableBalance, 0);
    }
}