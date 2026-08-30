using System.Net;
using Models.FundGoals;
using Tests.AccountingPeriods;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.FundGoals;

/// <summary>
/// Covers Fund Goal read contracts that are not exercised by configuration mutations.
/// </summary>
public sealed class FundGoalReadContractTests
{
    /// <summary>
    /// Returns availability for an existing Fund Goal and Not Found for missing Fund Goal resources.
    /// </summary>
    [Fact]
    public async Task GoalReadEndpointsExposeAvailabilityAndMissingResourceContracts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        FundGoalModel goal = await test.Api.GetAsync<FundGoalModel>($"/fund-goals/{groceries.Goal.Id}");
        FundAvailabilityModel availability = await test.Api.GetAsync<FundAvailabilityModel>($"/fund-goals/{groceries.Goal.Id}/availability");
        using HttpResponseMessage missingGoal = await test.Api.GetResponseAsync($"/fund-goals/{Guid.NewGuid()}");
        using HttpResponseMessage missingAvailability = await test.Api.GetResponseAsync($"/fund-goals/{Guid.NewGuid()}/availability");
        using HttpResponseMessage missingUpdate = await test.Api.PostResponseAsync($"/fund-goals/{Guid.NewGuid()}", new UpdateFundGoalModel());
        using HttpResponseMessage missingFund = await test.Api.GetResponseAsync($"/fund-goals/fund/{Guid.NewGuid()}");

        Assert.Equal(groceries.Goal.Id, goal.Id);
        Assert.Equal(0m, availability.AvailableBalance);
        Assert.Equal(0m, availability.AvailableBalanceIncludingPending);
        Assert.Equal(HttpStatusCode.NotFound, missingGoal.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingAvailability.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingFund.StatusCode);
    }
}
