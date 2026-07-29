using Domain.FundGoals;

namespace Tests.FundGoals;

/// <summary>
/// Covers pure Fund Goal contribution recommendation rules.
/// </summary>
public sealed class FundGoalProgressServiceTests
{
    /// <summary>
    /// Applies regular contributions and funded-balance bounds to the recommendation.
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
        FundGoalProgress progress = FundGoalProgressService.Calculate(100m, 40m, 130m, 25m, 150m, 200m, 125m);

        Assert.True(progress.AvailableBalance.IsSatisfied);
        Assert.NotNull(progress.Contribution);
        Assert.Equal(50m, progress.Contribution.TargetAmount);
        Assert.NotNull(progress.FundedBalance);
        Assert.NotNull(progress.EndingBalance);
    }

    /// <summary>
    /// Omits optional sections when the corresponding Fund Goal configuration is absent.
    /// </summary>
    [Fact]
    public void CalculateOmitsUnconfiguredProgressSections()
    {
        FundGoalProgress progress = FundGoalProgressService.Calculate(0m, 0m, -10m, null, null, null, null);

        Assert.False(progress.AvailableBalance.IsSatisfied);
        Assert.Equal(10m, progress.AvailableBalance.Shortfall);
        Assert.Null(progress.Contribution);
        Assert.Null(progress.FundedBalance);
        Assert.Null(progress.EndingBalance);
    }

    /// <summary>
    /// Reports funded and ending balance status on both sides of configured targets.
    /// </summary>
    [Fact]
    public void CalculateReportsFundedAndEndingBalanceStatuses()
    {
        FundGoalProgress below = FundGoalProgressService.Calculate(100m, 20m, 80m, null, 150m, 200m, 100m);
        FundGoalProgress above = FundGoalProgressService.Calculate(100m, 150m, 120m, null, 150m, 200m, 100m);

        Assert.Equal(FundedBalanceStatus.BelowMinimum, below.FundedBalance!.Status);
        Assert.Equal(30m, below.FundedBalance.AmountBelowMinimum);
        Assert.Equal(EndingBalanceStatus.BelowTarget, below.EndingBalance!.Status);
        Assert.Equal(FundedBalanceStatus.AboveMaximum, above.FundedBalance!.Status);
        Assert.Equal(50m, above.FundedBalance.AmountAboveMaximum);
        Assert.Equal(EndingBalanceStatus.AboveTarget, above.EndingBalance!.Status);
    }
}