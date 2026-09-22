using Models;
using Models.Locations;
using Models.Transactions;
using Models.Transactions.Types;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Pages;

/// <summary>
/// Characterizes the location workspace, detail, and trends reads.
/// </summary>
public sealed class LocationPageReadTests
{
    /// <summary>
    /// Lists locations created by transaction counterparties.
    /// </summary>
    [Fact]
    public async Task WorkspaceListsTransactionLocation()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();

        CollectionModel<LocationModel> locations = await test.Api.GetAsync<CollectionModel<LocationModel>>(
            "/locations?limit=500");

        Assert.Contains(locations.Items, item => item.Name == "Market");
    }

    /// <summary>
    /// Keeps recent transactions and cash flow totals scoped to the selected location.
    /// </summary>
    [Fact]
    public async Task DetailReturnsMatchingTransactionsAndCashFlow()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 7, 15));
        CollectionModel<LocationModel> locations = await test.Api.GetAsync<CollectionModel<LocationModel>>("/locations");
        LocationModel market = Assert.Single(locations.Items, item => item.Name == "Market");

        CollectionModel<TransactionModel> transactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.locationIds={market.Id}&limit=5");
        TransactionsInDateRangeModel cashFlow = await test.Api.GetAsync<TransactionsInDateRangeModel>(
            $"/transactions/date-range?range.start=2026-07-01&range.end=2026-07-31&filter.locationIds={market.Id}&limit=500");

        Assert.Equal(spending.Id, Assert.Single(transactions.Items).Id);
        Assert.Equal(20m, cashFlow.LocationOutgoingAmount);
        Assert.Equal(0m, cashFlow.LocationIncomingAmount);
    }

    /// <summary>
    /// Keeps a location's trends total independent of its paginated transaction rows.
    /// </summary>
    [Fact]
    public async Task TrendsKeepsCashFlowTotalsWhenTransactionsArePaged()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle first = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        TransactionHandle second = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 16)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(first, cash, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(second, cash, new DateOnly(2026, 7, 16));
        CollectionModel<LocationModel> locations = await test.Api.GetAsync<CollectionModel<LocationModel>>("/locations?limit=500");
        LocationModel market = Assert.Single(locations.Items, item => item.Name == "Market");

        TransactionsInDateRangeModel trends = await test.Api.GetAsync<TransactionsInDateRangeModel>(
            $"/transactions/date-range?range.start=2026-07-01&range.end=2026-07-31&filter.locationIds={market.Id}&limit=1&offset=0");

        Assert.Equal(2, trends.Transactions.TotalCount);
        _ = Assert.Single(trends.Transactions.Items);
        Assert.Equal(30m, trends.LocationOutgoingAmount);
    }
}
