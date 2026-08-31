using Domain.FundGoals;

namespace Tests.FundGoals;

/// <summary>
/// Covers pure Fund Goal contribution recommendation rules.
/// </summary>
public sealed class FundGoalProgressServiceTests
{
    /// <summary>
    /// Applies regular contributions and ending-balance bounds to the recommendation.
    /// </summary>
    [Fact]
    public void CalculateRecommendedContributionAppliesBounds()
    {
        Assert.Equal(25m, FundGoalProgressService.CalculateRecommendedContribution(100m, 25m, null, null));
        Assert.Equal(50m, FundGoalProgressService.CalculateRecommendedContribution(100m, 25m, 150m, null));
        Assert.Equal(50m, FundGoalProgressService.CalculateRecommendedContribution(100m, 100m, null, 150m));
        Assert.Equal(0m, FundGoalProgressService.CalculateRecommendedContribution(100m, -25m, null, null));
    }

    /// <summary>
    /// Exposes only progress sections configured by the Fund Goal.
    /// </summary>
    [Fact]
    public void CalculateIncludesConfiguredProgressSections()
    {
        FundGoalProgress progress = FundGoalProgressService.Calculate(100m, 40m, 130m, 25m, 150m, 200m);

        Assert.True(progress.AvailableBalance.IsSatisfied);
        Assert.NotNull(progress.Contribution);
        Assert.Equal(50m, progress.Contribution.TargetAmount);
        Assert.NotNull(progress.EndingBalance);
    }

    /// <summary>
    /// Counts only regular assignments toward regular-contribution progress.
    /// </summary>
    [Fact]
    public void CalculateSeparatesExtraFundingFromRegularContribution()
    {
        FundGoalProgress progress = FundGoalProgressService.Calculate(
            100m,
            0m,
            250m,
            200m,
            200m,
            300m);

        Assert.NotNull(progress.Contribution);
        Assert.Equal(0m, progress.Contribution.AssignedAmount);
        Assert.Equal(200m, progress.Contribution.RemainingAmount);
        Assert.NotNull(progress.EndingBalance);
        Assert.Equal(250m, progress.EndingBalance.CurrentBalance);
        Assert.Equal(FundGoalEndingBalanceStatus.WithinRange, progress.EndingBalance.Status);
    }

    /// <summary>
    /// Omits optional sections when the corresponding Fund Goal configuration is absent.
    /// </summary>
    [Fact]
    public void CalculateOmitsUnconfiguredProgressSections()
    {
        FundGoalProgress progress = FundGoalProgressService.Calculate(0m, 0m, -10m, null, null, null);

        Assert.False(progress.AvailableBalance.IsSatisfied);
        Assert.Equal(10m, progress.AvailableBalance.Shortfall);
        Assert.Null(progress.Contribution);
        Assert.Null(progress.EndingBalance);
    }

    /// <summary>
    /// Reports ending-balance status on both sides of configured bounds.
    /// </summary>
    [Fact]
    public void CalculateReportsFundedAndEndingBalanceStatuses()
    {
        FundGoalProgress below = FundGoalProgressService.Calculate(100m, 20m, 80m, null, 150m, 200m);
        FundGoalProgress above = FundGoalProgressService.Calculate(100m, 150m, 250m, null, 150m, 200m);

        Assert.Equal(FundGoalEndingBalanceStatus.BelowMinimum, below.EndingBalance!.Status);
        Assert.Equal(70m, below.EndingBalance.AmountBelowMinimum);
        Assert.Equal(FundGoalEndingBalanceStatus.AboveMaximum, above.EndingBalance!.Status);
        Assert.Equal(50m, above.EndingBalance.AmountAboveMaximum);
    }
}
