using System.Net;
using Models;
using Models.FundGoals;
using Tests.AccountingPeriods;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.FundGoals;

/// <summary>
/// Covers Fund Goal configuration, progress, and lookup endpoints.
/// </summary>
public sealed class FundGoalEndpointTests
{
    /// <summary>
    /// Defaults new goals and cleared minimums to zero without enabling contributions.
    /// </summary>
    [Fact]
    public async Task DefaultMinimumIsZeroOnCreateAndUpdate()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle fund = await test.Funds.Create("Reserve").In(july).CreateAsync();
        FundGoalModel created = await test.Api.GetAsync<FundGoalModel>($"/fund-goals/fund/{fund.Id}?accountingPeriodId={july.Id}");
        Assert.Equal(0m, created.MinimumEndingBalance);

        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{fund.Goal.Id}", new UpdateFundGoalModel
        {
            MinimumEndingBalance = 100m,
        });
        FundGoalModel updated = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{fund.Goal.Id}", new UpdateFundGoalModel
        {
            MinimumEndingBalance = null,
            MaximumEndingBalance = 200m,
        });
        FundGoalProgressModel progress = await test.Api.GetAsync<FundGoalProgressModel>($"/fund-goals/{fund.Goal.Id}/progress/{july.Id}");

        Assert.Equal(0m, updated.MinimumEndingBalance);
        Assert.Equal(200m, updated.MaximumEndingBalance);
        Assert.Equal(0m, progress.EndingBalance.MinimumBalance);
        Assert.Equal(FundGoalEndingBalanceStatusModel.WithinRange, progress.EndingBalance.Status);
        Assert.Null(progress.Contribution);
    }

    /// <summary>
    /// Persists Fund Goal configuration and exposes it through progress and list projections.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncExposesConfiguredGoalProgress()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        FundGoalModel updated = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            PlannedMonthlyContribution = 50m,
            MinimumEndingBalance = 100m,
            MaximumEndingBalance = 200m
        });
        FundGoalProgressModel progress = await test.Api.GetAsync<FundGoalProgressModel>($"/fund-goals/{groceries.Goal.Id}/progress/{july.Id}");
        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>($"/fund-goals?filter.accountingPeriodIds={july.Id}");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync($"/fund-goals/{Guid.NewGuid()}/progress/{july.Id}");

        Assert.Equal(50m, updated.PlannedMonthlyContribution);
        Assert.NotNull(progress.Contribution);
        Assert.Equal(50m, progress.Contribution.PlannedAmount);
        Assert.Equal(50m, progress.Contribution.ExpectedAmount);
        Assert.Equal(0m, progress.Contribution.AmountReducedByMaximumEndingBalance);
        Assert.Contains(goals.Items, goal => goal.Id == groceries.Goal.Id);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Clears the contribution override when no maximum ending balance is configured.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncClearsContributionOverrideWithoutMaximumEndingBalance()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        FundGoalModel updated = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>(
            $"/fund-goals/{groceries.Goal.Id}",
            new UpdateFundGoalModel
            {
                MaximumEndingBalance = null,
                AllowExpectedContributionAboveMaximum = true,
            });

        Assert.Null(updated.MaximumEndingBalance);
        Assert.False(updated.AllowExpectedContributionAboveMaximum);
    }

    /// <summary>
    /// Rejects negative Fund Goal quantities and inverted ending-balance bounds at the mutation boundary.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncRejectsInvalidGoalConfiguration()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        using HttpResponseMessage negative = await test.Api.PostResponseAsync($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            PlannedMonthlyContribution = -1m
        });
        using HttpResponseMessage inverted = await test.Api.PostResponseAsync($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            MinimumEndingBalance = 200m,
            MaximumEndingBalance = 100m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, negative.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, inverted.StatusCode);
    }

    /// <summary>
    /// Prevents configuration changes to a Fund Goal owned by a closed Accounting Period.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncRejectsGoalsInClosedAccountingPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        await test.Api.PostAsync($"/accounting-periods/{july.Id}/close");

        using HttpResponseMessage response = await test.Api.PostResponseAsync($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            PlannedMonthlyContribution = 50m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
