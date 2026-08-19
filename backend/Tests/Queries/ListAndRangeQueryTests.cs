using System.Net;
using Models;
using Models.Accounts;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Infrastructure;

namespace Tests.Queries;

/// <summary>
/// Covers paged list and accounting-period range read contracts.
/// </summary>
public sealed class ListAndRangeQueryTests
{
    /// <summary>
    /// Returns paged account and fund lists with an accurate total count.
    /// </summary>
    [Fact]
    public async Task ListsSupportLimitAndReportTotalCount()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Alpha").CreateAsync();
        _ = await test.Accounts.Onboard("Beta").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        _ = await test.Funds.Create("Food").In(july).CreateAsync();
        _ = await test.Funds.Create("Travel").In(july).CreateAsync();

        CollectionModel<AccountModel> accounts = await test.Api.GetAsync<CollectionModel<AccountModel>>("/accounts?limit=1");
        CollectionModel<FundModel> funds = await test.Api.GetAsync<CollectionModel<FundModel>>("/funds?limit=1");

        _ = Assert.Single(accounts.Items);
        Assert.Equal(2, accounts.TotalCount);
        _ = Assert.Single(funds.Items);
        Assert.Equal(3, funds.TotalCount); // Includes the period's unassigned Fund.
    }

    /// <summary>
    /// Rejects a noncontiguous accounting-period range.
    /// </summary>
    [Fact]
    public async Task AccountingPeriodRangeRejectsMissingPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage response = await test.Api.GetResponseAsync(
            $"/accounting-periods/range?range.start={july.Id}&range.end={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
