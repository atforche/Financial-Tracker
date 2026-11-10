using System.Net;
using Models.Accounts;
using Models.Locations;

namespace Tests.Locations;

/// <summary>
/// Covers the Location range summaries behind the Location workspace and detail pages.
/// </summary>
public sealed class LocationRangeTests
{
    /// <summary>
    /// Ranks Locations by spending within the range and totals across them.
    /// </summary>
    [Fact]
    public async Task DateRangeRanksLocationsAndTotalsThem()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").CreateAsync();
        AccountingPeriodHandle june = await test.Periods.Create(2026, 6).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(june).CreateAsync();
        _ = await test.Transactions.Spending().In(june).On(new DateOnly(2026, 6, 20)).For(30m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 5)).For(50m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 9)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 12)).For(90m).From(cash).To("Hardware", groceries).CreateAsync();
        _ = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(400m).From("Employer").To(savings, groceries).CreateAsync();

        LocationsInRangeModel range = await test.Api.GetAsync<LocationsInRangeModel>(
            "/locations/date-range?range.start=2026-07-01&range.end=2026-07-31");

        Assert.Equal(["Hardware", "Market", "Employer"], range.Locations.Items.Select(location => location.Name));
        LocationRangeSummaryModel market = range.Locations.Items.Single(location => location.Name == "Market");
        Assert.Equal(70m, market.Spent);
        Assert.Equal(2, market.TransactionCount);
        Assert.Equal(new DateOnly(2026, 7, 9), market.LastActivity);
        Assert.Equal(400m, range.Locations.Items.Single(location => location.Name == "Employer").Received);
        Assert.Equal(160m, range.TotalSpent);
        Assert.Equal(400m, range.TotalReceived);
        Assert.Equal(3, range.ActiveLocationCount);

        LocationsInRangeModel byName = await test.Api.GetAsync<LocationsInRangeModel>(
            "/locations/date-range?range.start=2026-07-01&range.end=2026-07-31&sort=Name&nameSearch=ar");
        Assert.Equal(["Hardware", "Market"], byName.Locations.Items.Select(location => location.Name));
    }

    /// <summary>
    /// Counts the spending transactions apart from refunds, so an average spent covers only purchases.
    /// </summary>
    [Fact]
    public async Task CountsSpendingTransactionsApartFromRefunds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 5)).For(100m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Refund().In(july).On(new DateOnly(2026, 7, 9)).For(25m).From("Market").To(cash, groceries).CreateAsync();

        LocationsInRangeModel range = await test.Api.GetAsync<LocationsInRangeModel>(
            "/locations/date-range?range.start=2026-07-01&range.end=2026-07-31");

        LocationRangeSummaryModel market = Assert.Single(range.Locations.Items);
        Assert.Equal(2, market.TransactionCount);
        Assert.Equal(1, market.SpendingTransactionCount);
    }

    /// <summary>
    /// Summarizes one Location by period, with the Accounts and Funds that paid it.
    /// </summary>
    [Fact]
    public async Task DetailRangeBreaksDownMonthsAccountsAndFunds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountHandle card = await test.Accounts.Onboard("Card").WithType(AccountTypeModel.CreditCard).CreateAsync();
        AccountingPeriodHandle june = await test.Periods.Create(2026, 6).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(june).CreateAsync();
        FundHandle household = await test.Funds.Create("Household").In(june).CreateAsync();
        _ = await test.Transactions.Spending().In(june).On(new DateOnly(2026, 6, 10)).For(40m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(60m).From(card).To("Market", household).CreateAsync();

        LocationsInRangeModel all = await test.Api.GetAsync<LocationsInRangeModel>(
            $"/locations/accounting-period-range?range.start={june.Id}&range.end={july.Id}");
        Guid marketId = Assert.Single(all.Locations.Items).Id;

        LocationDetailRangeModel detail = await test.Api.GetAsync<LocationDetailRangeModel>(
            $"/locations/{marketId}/accounting-period-range?range.start={june.Id}&range.end={july.Id}");

        Assert.Equal(100m, detail.Summary.Spent);
        Assert.Equal([40m, 60m], detail.Months.Select(month => month.Spent));
        Assert.Equal([("Card", 60m), ("Cash", 40m)], detail.Accounts.Select(share => (share.Name, share.Amount)));
        Assert.Equal([("Household", 60m), ("Groceries", 40m)], detail.Funds.Select(share => (share.Name, share.Amount)));

        HttpResponseMessage missing = await test.Api.GetResponseAsync(
            $"/locations/{Guid.NewGuid()}/date-range?range.start=2026-07-01&range.end=2026-07-31");
        HttpResponseMessage openEnded = await test.Api.GetResponseAsync("/locations/date-range?range.start=2026-07-01");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, openEnded.StatusCode);
    }
}
