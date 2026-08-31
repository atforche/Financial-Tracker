using Domain.FundGoals;

namespace Tests.FundGoals;

/// <summary>
/// Covers pure Fund Goal contribution recommendation rules.
/// </summary>
public sealed class FundGoalProgressServiceTests
{
    /// <summary>
    /// Applies the planned monthly contribution and maximum ending-balance cap to the expectation.
    /// </summary>
    [Fact]
    public void CalculateExpectedContributionAppliesBounds()
    {
        Assert.Equal(25m, FundGoalProgressService.CalculateExpectedContribution(100m, 0m, 25m, null));
        Assert.Equal(25m, FundGoalProgressService.CalculateExpectedContribution(100m, 10m, 25m, 150m));
        Assert.Equal(50m, FundGoalProgressService.CalculateExpectedContribution(100m, 0m, 100m, 150m));
        Assert.Equal(25m, FundGoalProgressService.CalculateExpectedContribution(100m, 60m, 25m, null));
        Assert.Equal(0m, FundGoalProgressService.CalculateExpectedContribution(100m, 0m, -25m, null));
    }

    /// <summary>
    /// Does not recommend a contribution when the current balance exceeds the maximum ending balance.
    /// </summary>
    [Fact]
    public void CalculateExpectedContributionReturnsZeroAboveMaximumEndingBalance() =>
        Assert.Equal(0m, FundGoalProgressService.CalculateExpectedContribution(175m, 10m, 25m, 150m));

    /// <summary>
    /// Exposes only progress sections configured by the Fund Goal.
    /// </summary>
    [Fact]
    public void CalculateIncludesConfiguredProgressSections()
    {
        FundGoalProgress progress = FundGoalProgressService.Calculate(40m, 130m, 25m, 150m, 200m);

        Assert.True(progress.AvailableBalance.IsSatisfied);
        Assert.NotNull(progress.Contribution);
        Assert.Equal(25m, progress.Contribution.ExpectedAmount);
        Assert.NotNull(progress.EndingBalance);
    }

    /// <summary>
    /// Counts only planned assignments toward planned-contribution progress.
    /// </summary>
    [Fact]
    public void CalculateSeparatesExtraFundingFromPlannedMonthlyContribution()
    {
        FundGoalProgress progress = FundGoalProgressService.Calculate(
            0m,
            250m,
            200m,
            200m,
            300m);

        Assert.NotNull(progress.Contribution);
        Assert.Equal(0m, progress.Contribution.AssignedAmount);
        Assert.Equal(50m, progress.Contribution.RemainingAmount);
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
        FundGoalProgress progress = FundGoalProgressService.Calculate(0m, -10m, null, null, null);

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
        FundGoalProgress below = FundGoalProgressService.Calculate(20m, 80m, null, 150m, 200m);
        FundGoalProgress above = FundGoalProgressService.Calculate(150m, 250m, null, 150m, 200m);

        Assert.Equal(FundGoalEndingBalanceStatus.BelowMinimum, below.EndingBalance!.Status);
        Assert.Equal(70m, below.EndingBalance.AmountBelowMinimum);
        Assert.Equal(FundGoalEndingBalanceStatus.AboveMaximum, above.EndingBalance!.Status);
        Assert.Equal(50m, above.EndingBalance.AmountAboveMaximum);
    }
}
