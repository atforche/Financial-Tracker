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
        decimal regularAssignedAmount,
        decimal currentAvailableBalance,
        decimal? regularContribution,
        decimal? minimumEndingBalance,
        decimal? maximumEndingBalance)
    {
        decimal recommendedContribution = CalculateRecommendedContribution(
            openingAvailableBalance,
            regularContribution,
            minimumEndingBalance,
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
    /// Calculates the recommended contribution after applying ending-balance constraints.
    /// </summary>
    public static decimal CalculateRecommendedContribution(
        decimal openingAvailableBalance,
        decimal? regularContribution,
        decimal? minimumEndingBalance,
        decimal? maximumEndingBalance)
    {
        decimal recommendedBalance = openingAvailableBalance + (regularContribution ?? 0);

        if (minimumEndingBalance is decimal minimum)
        {
            recommendedBalance = Math.Max(recommendedBalance, minimum);
        }
        if (maximumEndingBalance is decimal maximum)
        {
            recommendedBalance = Math.Min(recommendedBalance, maximum);
        }
        return Math.Max(recommendedBalance - openingAvailableBalance, 0);
    }
}
