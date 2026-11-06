using System.Net;
using Models;
using Models.FundGoals;

namespace Tests.FundGoals;

/// <summary>
/// Covers Fund Goal configuration, progress, and lookup endpoints.
/// </summary>
public sealed class FundGoalEndpointTests
{
    /// <summary>
    /// Returns a Fund's goals and progress across a period range, oldest first.
    /// </summary>
    [Fact]
    public async Task HistoryReturnsGoalsAcrossPeriodRange()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Periods.Create(2026, 8).CreateAsync();
        AccountingPeriodHandle september = await test.Periods.Create(2026, 9).CreateAsync();
        AccountingPeriodHandle october = await test.Periods.Create(2026, 10).CreateAsync();
        FundHandle fund = await test.Funds.Create("Vacation").In(september).CreateAsync();

        IReadOnlyList<FundGoalPeriodProgressModel> history = await test.Api.GetAsync<List<FundGoalPeriodProgressModel>>(
            $"/fund-goals/fund/{fund.Id}/history?range.start={september.Id}&range.end={october.Id}");

        Assert.Equal([9, 10], history.Select(item => item.Goal.AccountingPeriod!.Month));
        Assert.All(history, item => Assert.Equal(fund.Id, item.Goal.Fund.Id));
    }

    /// <summary>
    /// Treats SQL pattern characters in fund-name searches as literal text.
    /// </summary>
    [Theory]
    [InlineData("%", "Budget%Plan")]
    [InlineData("_", "Budget_Plan")]
    [InlineData("\\", "Budget\\Plan")]
    public async Task ListTreatsNameSearchAsLiteralText(string search, string expectedName)
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle period = await test.Periods.Create(2026, 7).CreateAsync();
        foreach (string name in new[] { "Budget%Plan", "Budget_Plan", "Budget\\Plan", "BudgetXPlan" })
        {
            _ = await test.Funds.Create(name).In(period).CreateAsync();
        }

        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>(
            $"/fund-goals?filter.accountingPeriodIds={period.Id}&filter.nameSearch={Uri.EscapeDataString(search)}");

        Assert.Equal(expectedName, Assert.Single(goals.Items).Fund.Name);
        Assert.Equal(1, goals.TotalCount);
    }

    /// <summary>
    /// Filters Fund Goals by a partial fund name before paging the results.
    /// </summary>
    [Fact]
    public async Task ListFiltersByFundNameWithinAccountingPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle julyReserve = await test.Funds.Create("Holiday Reserve").In(july).CreateAsync();
        _ = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Funds.Create("Other Reserve").In(august).CreateAsync();

        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>(
            $"/fund-goals?filter.accountingPeriodIds={july.Id}&filter.nameSearch=reserve&limit=1");

        FundGoalModel matchedGoal = Assert.Single(goals.Items);
        Assert.Equal(julyReserve.Id, matchedGoal.Fund.Id);
        Assert.Equal(1, goals.TotalCount);
    }

    /// <summary>
    /// Sorts Fund Goals by each configurable planning field.
    /// </summary>
    [Fact]
    public async Task ListSortsByConfigurableFields()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle alpha = await test.Funds.Create("Alpha").In(july).CreateAsync();
        FundHandle beta = await test.Funds.Create("Beta").In(july).CreateAsync();
        FundHandle gamma = await test.Funds.Create("Gamma").In(july).CreateAsync();

        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{alpha.Goal.Id}", new UpdateFundGoalModel
        {
            PlannedMonthlyContribution = 10m,
            MinimumEndingBalance = 300m,
            MaximumEndingBalance = 500m,
        });
        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{beta.Goal.Id}", new UpdateFundGoalModel
        {
            PlannedMonthlyContribution = 30m,
            MinimumEndingBalance = 100m,
            MaximumEndingBalance = 400m,
            AllowExpectedContributionAboveMaximum = true,
        });
        _ = await test.Api.PostAsync<UpdateFundGoalModel, FundGoalModel>($"/fund-goals/{gamma.Goal.Id}", new UpdateFundGoalModel
        {
            PlannedMonthlyContribution = 20m,
            MinimumEndingBalance = 200m,
            MaximumEndingBalance = 600m,
        });

        async Task<IReadOnlyList<string>> GetNamesAsync(FundGoalSortModel sort)
        {
            CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>(
                $"/fund-goals?filter.accountingPeriodIds={july.Id}&filter.fundIds={alpha.Id}&filter.fundIds={beta.Id}&filter.fundIds={gamma.Id}&sort={sort}");
            return goals.Items.Select(goal => goal.Fund.Name).ToList();
        }

        Assert.Equal(["Beta", "Gamma", "Alpha"], await GetNamesAsync(FundGoalSortModel.PlannedMonthlyContributionDescending));
        Assert.Equal(["Alpha", "Gamma", "Beta"], await GetNamesAsync(FundGoalSortModel.MinimumEndingBalanceDescending));
        Assert.Equal(["Gamma", "Alpha", "Beta"], await GetNamesAsync(FundGoalSortModel.MaximumEndingBalanceDescending));
    }

    /// <summary>
    /// Defaults new goals and cleared minimums to zero without enabling contributions.
    /// </summary>
    [Fact]
    public async Task DefaultMinimumIsZeroOnCreateAndUpdate()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle fund = await test.Funds.Create("Reserve").In(july).CreateAsync();
        FundGoalModel created = await test.Api.GetFundGoalAsync(fund.Id, july.Id);
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
        FundGoalProgressModel progress = await test.Api.GetFundGoalProgressAsync(fund.Goal.Id, july.Id);

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
        FundGoalProgressModel progress = await test.Api.GetFundGoalProgressAsync(groceries.Goal.Id, july.Id);
        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>($"/fund-goals?filter.accountingPeriodIds={july.Id}");

        Assert.Equal(50m, updated.PlannedMonthlyContribution);
        Assert.NotNull(progress.Contribution);
        Assert.Equal(50m, progress.Contribution.PlannedAmount);
        Assert.Equal(50m, progress.Contribution.ExpectedAmount);
        Assert.Equal(0m, progress.Contribution.AmountReducedByMaximumEndingBalance);
        Assert.Contains(goals.Items, goal => goal.Id == groceries.Goal.Id);
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
    /// Rejects negative quantities, inverted bounds, goals in closed periods, and missing goals or funds.
    /// </summary>
    [Fact]
    public async Task MutationsAndLookupsRejectInvalidGoals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(august).CreateAsync();
        FundHandle closed = await test.Funds.Create("Closed").In(july).CreateAsync();
        await test.Periods.CloseAsync(july);

        using HttpResponseMessage negative = await test.Api.PostResponseAsync($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel { PlannedMonthlyContribution = -1m });
        using HttpResponseMessage inverted = await test.Api.PostResponseAsync($"/fund-goals/{groceries.Goal.Id}", new UpdateFundGoalModel
        {
            MinimumEndingBalance = 200m,
            MaximumEndingBalance = 100m
        });
        using HttpResponseMessage closedPeriod = await test.Api.PostResponseAsync($"/fund-goals/{closed.Goal.Id}", new UpdateFundGoalModel { PlannedMonthlyContribution = 50m });
        using HttpResponseMessage missingUpdate = await test.Api.PostResponseAsync($"/fund-goals/{Guid.NewGuid()}", new UpdateFundGoalModel());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, negative.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, inverted.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closedPeriod.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingUpdate.StatusCode);
    }

    /// <summary>
    /// Filters goals by fund across every accounting period the fund belongs to.
    /// </summary>
    [Fact]
    public async Task ListFiltersByFundAcrossPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        _ = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle alpha = await test.Funds.Create("Alpha").In(july).CreateAsync();
        _ = await test.Funds.Create("Beta").In(july).CreateAsync();

        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>($"/fund-goals?filter.fundIds={alpha.Id}&sort=Fund");

        Assert.Equal(2, goals.TotalCount);
        Assert.All(goals.Items, goal => Assert.Equal(alpha.Id, goal.Fund.Id));
        Assert.Contains(goals.Items, goal => goal.Id == alpha.Goal.Id);
    }
}
