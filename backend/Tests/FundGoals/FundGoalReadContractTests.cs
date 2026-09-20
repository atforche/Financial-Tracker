using System.Net;
using Models.BalanceEvents;
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
    /// Returns Not Found for missing Fund Goal resources.
    /// </summary>
    [Fact]
    public async Task GoalReadEndpointsExposeAvailabilityAndMissingResourceContracts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        using HttpResponseMessage missingUpdate = await test.Api.PostResponseAsync($"/fund-goals/{Guid.NewGuid()}", new UpdateFundGoalModel());
        using HttpResponseMessage missingFund = await test.Api.GetResponseAsync($"/fund-goals/fund/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, missingUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingFund.StatusCode);

        BalanceEventTotalsModel totals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            $"/fund-goals/balance-events/accounting-period-range/totals?Range.Start={july.Id}&Range.End={july.Id}&Filter.FundIds={groceries.Id}");
        Assert.Equal(0m, totals.TotalInflow);
        Assert.Equal(0m, totals.TotalOutflow);
    }
}
