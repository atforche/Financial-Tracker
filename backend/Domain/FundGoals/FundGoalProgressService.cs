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
        decimal amountAssignedToExpectedContribution,
        decimal currentAvailableBalance,
        decimal? plannedMonthlyContribution,
        decimal? minimumEndingBalance,
        decimal? maximumEndingBalance)
    {
        decimal expectedContribution = CalculateExpectedContribution(
            currentAvailableBalance,
            amountAssignedToExpectedContribution,
            plannedMonthlyContribution,
            maximumEndingBalance);
        ContributionProgress? contribution = plannedMonthlyContribution != null
            || minimumEndingBalance != null
            || maximumEndingBalance != null
            ? new ContributionProgress(expectedContribution, amountAssignedToExpectedContribution)
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
    /// Calculates the expected contribution after applying the maximum ending-balance constraint.
    /// </summary>
    public static decimal CalculateExpectedContribution(
        decimal currentAvailableBalance,
        decimal currentContributions,
        decimal? plannedMonthlyContribution,
        decimal? maximumEndingBalance)
    {
        decimal expectedContribution = Math.Max(plannedMonthlyContribution ?? 0, 0);
        if (maximumEndingBalance is decimal maximum)
        {
            decimal contributions = Math.Max(currentContributions, 0);
            decimal availableBalance = maximum - currentAvailableBalance;
            expectedContribution = Math.Max(
                Math.Min(expectedContribution, contributions + availableBalance),
                0);
        }
        return expectedContribution;
    }
}
