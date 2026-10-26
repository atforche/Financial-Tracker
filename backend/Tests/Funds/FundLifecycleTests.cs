using System.Net;
using Models.FundGoals;
using Models.Funds;

namespace Tests.Funds;

/// <summary>
/// Covers creating, updating, and deleting funds.
/// </summary>
public sealed class FundLifecycleTests
{
    /// <summary>
    /// Updates and deletes funds while rejecting missing funds.
    /// </summary>
    [Fact]
    public async Task UpdateAndDeleteAsyncReturnResourceContracts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle fund = await test.Funds.Create("Travel").In(july).CreateAsync();

        FundModel updated = await test.Api.PostAsync<UpdateFundModel, FundModel>($"/funds/{fund.Id}", new UpdateFundModel { Name = "Holiday", Description = "Updated" });
        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/funds/{fund.Id}");
        using HttpResponseMessage missingUpdate = await test.Api.PostResponseAsync($"/funds/{Guid.NewGuid()}", new UpdateFundModel { Name = "Missing", Description = "Missing" });
        using HttpResponseMessage missingDelete = await test.Api.DeleteResponseAsync($"/funds/{Guid.NewGuid()}");

        Assert.Equal("Holiday", updated.Name);
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingDelete.StatusCode);
    }

    /// <summary>
    /// Rejects funds in missing or closed accounting periods and goal bounds that cannot describe a valid range.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRejectsInvalidPeriodsAndBounds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        await test.Periods.CloseAsync(july);

        using HttpResponseMessage missingPeriod = await test.Api.PostResponseAsync("/funds", new CreateFundModel
        {
            Name = "Missing period",
            Description = "Missing period",
            AccountingPeriodId = Guid.NewGuid()
        });
        using HttpResponseMessage closedPeriod = await test.Api.PostResponseAsync("/funds", new CreateFundModel
        {
            Name = "Closed period",
            Description = "Closed period",
            AccountingPeriodId = july.Id
        });
        using HttpResponseMessage invertedBounds = await test.Api.PostResponseAsync("/funds", new CreateFundModel
        {
            Name = "Invalid bounds",
            Description = "Invalid bounds",
            AccountingPeriodId = august.Id,
            MinimumEndingBalance = 200m,
            MaximumEndingBalance = 100m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingPeriod.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closedPeriod.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invertedBounds.StatusCode);
    }

    /// <summary>
    /// Keeps a fund referenced by a transaction after a rejected deletion request.
    /// </summary>
    [Fact]
    public async Task DeleteAsyncRejectsFundsReferencedByTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();

        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/funds/{groceries.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, delete.StatusCode);
        await test.AssertBalanceAsync(groceries, 0m, -10m);
    }

    /// <summary>
    /// Adds a fund and its goals to, and removes them from, every later accounting period.
    /// </summary>
    [Fact]
    public async Task CreateAndDeleteAsyncMaintainMembershipAndGoalsAcrossLaterPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle fund = await test.Funds.Create("Travel").In(july).CreateAsync();
        string rangeUri = $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}";
        FundGoalModel augustGoal = await test.Api.GetAsync<FundGoalModel>($"/fund-goals/fund/{fund.Id}?accountingPeriodId={august.Id}");

        Assert.Contains((await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(rangeUri)).Funds.Items, item => item.Id == fund.Id);
        Assert.NotEqual(fund.Goal.Id, augustGoal.Id);

        await test.Api.DeleteAsync($"/funds/{fund.Id}");
        using HttpResponseMessage missingJulyGoal = await test.Api.GetResponseAsync($"/fund-goals/fund/{fund.Id}?accountingPeriodId={july.Id}");
        using HttpResponseMessage missingAugustGoal = await test.Api.GetResponseAsync($"/fund-goals/fund/{fund.Id}?accountingPeriodId={august.Id}");

        Assert.DoesNotContain((await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(rangeUri)).Funds.Items, item => item.Id == fund.Id);
        Assert.Equal(HttpStatusCode.NotFound, missingJulyGoal.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingAugustGoal.StatusCode);
    }
}
