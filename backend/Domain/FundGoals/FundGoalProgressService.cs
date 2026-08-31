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
        decimal regularAssignedAmount,
        decimal currentAvailableBalance,
        decimal? regularContribution,
        decimal? minimumEndingBalance,
        decimal? maximumEndingBalance)
    {
        decimal recommendedContribution = CalculateRecommendedContribution(
            currentAvailableBalance,
            regularAssignedAmount,
            regularContribution,
            maximumEndingBalance);
        ContributionProgress? contribution = regularContribution != null
            || minimumEndingBalance != null
            || maximumEndingBalance != null
            ? new ContributionProgress(recommendedContribution, regularAssignedAmount)
            : null;
        FundGoalEndingBalanceProgress? endingBalance = minimumEndingBalance != null
            || maximumEndingBalance != null
            ? new FundGoalEndingBalanceProgress(
                currentAvailableBalance,
                minimumEndingBalance,
                maximumEndingBalance)
            : null;

        return new FundGoalProgress(
            new AvailableBalanceProgress(currentAvailableBalance),
            contribution,
            endingBalance);
    }

    /// <summary>
    /// Calculates the total recommended contribution after applying the maximum ending-balance constraint.
    /// </summary>
    public static decimal CalculateRecommendedContribution(
        decimal currentAvailableBalance,
        decimal currentContributions,
        decimal? regularContribution,
        decimal? maximumEndingBalance)
    {
        decimal contributions = Math.Max(currentContributions, 0);
        decimal remainingContribution = Math.Max((regularContribution ?? 0) - contributions, 0);
        if (maximumEndingBalance is decimal maximum)
        {
            remainingContribution = Math.Min(
                remainingContribution,
                Math.Max(maximum - currentAvailableBalance, 0));
        }
        return contributions + remainingContribution;
    }
}
