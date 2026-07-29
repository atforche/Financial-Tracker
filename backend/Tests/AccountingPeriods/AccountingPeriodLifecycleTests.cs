using System.Net;
using Models;
using Models.AccountingPeriods;
using Models.Funds;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.AccountingPeriods;

/// <summary>
/// Covers accounting-period sequencing and lifecycle constraints through the API.
/// </summary>
public sealed class AccountingPeriodLifecycleTests
{
    /// <summary>
    /// Rejects duplicate and noncontiguous periods while accepting the next period.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRequiresUniqueContiguousPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage duplicate = await test.Api.PostResponseAsync("/accounting-periods", new CreateAccountingPeriodModel
        {
            Year = 2026,
            Month = 7
        });
        using HttpResponseMessage gap = await test.Api.PostResponseAsync("/accounting-periods", new CreateAccountingPeriodModel
        {
            Year = 2026,
            Month = 9
        });
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, gap.StatusCode);
        Assert.NotEqual(july.Id, august.Id);
    }

    /// <summary>
    /// Initializes the required Unassigned Fund when the first accounting period is created.
    /// </summary>
    [Fact]
    public async Task CreateFirstAsyncInitializesUnassignedFund()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Periods.Create(2026, 7).CreateAsync();

        CollectionModel<FundModel> funds = await test.Api.GetAsync<CollectionModel<FundModel>>("/funds");

        Assert.Contains(funds.Items, fund => fund.Name == "Unassigned");
    }

    /// <summary>
    /// Allows a posted period to close and reopen, while preventing duplicate close operations.
    /// </summary>
    [Fact]
    public async Task CloseAndReopenAsyncEnforcePeriodState()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage close = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/close", new { });
        using HttpResponseMessage repeatClose = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/close", new { });
        using HttpResponseMessage reopen = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/reopen", new { });

        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, repeatClose.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reopen.StatusCode);
    }

    /// <summary>
    /// Refuses to close a period containing an unposted transaction.
    /// </summary>
    [Fact]
    public async Task CloseAsyncRejectsUnpostedTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();

        using HttpResponseMessage response = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/close", new { });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>
    /// Requires prior periods to close first and prevents deletion that would create a gap.
    /// </summary>
    [Fact]
    public async Task PeriodOrderingPreventsClosingLaterAndDeletingEarlierPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        using HttpResponseMessage closeLater = await test.Api.PostResponseAsync($"/accounting-periods/{august.Id}/close", new { });
        using HttpResponseMessage deleteEarlier = await test.Api.DeleteResponseAsync($"/accounting-periods/{july.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, closeLater.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, deleteEarlier.StatusCode);
    }

    /// <summary>
    /// Deletes the latest empty period and its period-owned unassigned fund.
    /// </summary>
    [Fact]
    public async Task DeleteAsyncRemovesTheLatestEmptyPeriodAndItsUnassignedFund()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        CollectionModel<FundModel> before = await test.Api.GetAsync<CollectionModel<FundModel>>("/funds");
        using HttpResponseMessage deleted = await test.Api.DeleteResponseAsync($"/accounting-periods/{july.Id}");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync($"/accounting-periods/{july.Id}");
        CollectionModel<FundModel> after = await test.Api.GetAsync<CollectionModel<FundModel>>("/funds");

        Assert.Contains(before.Items, fund => fund.Name == "Unassigned");
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.DoesNotContain(after.Items, fund => fund.Name == "Unassigned");
    }
}