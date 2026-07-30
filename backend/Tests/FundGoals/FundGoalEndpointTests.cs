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
    /// Persists goal configuration and exposes it through progress and list projections.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncExposesConfiguredGoalProgress()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        FundGoalModel updated = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            RegularContribution = 50m,
            MinimumFundedBalance = 100m,
            MaximumFundedBalance = 200m,
            TargetEndingBalance = 150m
        });
        FundGoalProgressModel progress = await test.Api.GetAsync<FundGoalProgressModel>($"/fund-goals/{groceries.Goal.Id}/progress/{july.Id}");
        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>($"/fund-goals?filter.accountingPeriodIds={july.Id}");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync($"/fund-goals/{Guid.NewGuid()}/progress/{july.Id}");

        Assert.Equal(50m, updated.RegularContribution);
        Assert.NotNull(progress.Contribution);
        Assert.Contains(goals.Items, goal => goal.Id == groceries.Goal.Id);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Rejects negative Fund Goal quantities and inverted funded-balance bounds at the mutation boundary.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncRejectsInvalidGoalConfiguration()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        using HttpResponseMessage negative = await test.Api.PostResponseAsync($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            RegularContribution = -1m
        });
        using HttpResponseMessage inverted = await test.Api.PostResponseAsync($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            MinimumFundedBalance = 200m,
            MaximumFundedBalance = 100m
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
            RegularContribution = 50m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}