using System.Net;
using Models;
using Models.Transactions.Types;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Queries;

/// <summary>
/// Covers compound Transaction query filters and common range validation behavior.
/// </summary>
public sealed class QueryFilterAndRangeValidationTests
{
    /// <summary>
    /// Applies Account, Fund, period, and type filters before sorting the Transaction page.
    /// </summary>
    [Fact]
    public async Task TransactionListCombinesFiltersAndSortsByDestination()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle travel = await test.Funds.Create("Travel").In(july).CreateAsync();
        TransactionHandle groceriesTransaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 16)).For(30m).From(cash).To("Trip", travel).CreateAsync();
        _ = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 17)).For(10m).From(cash).To(savings).CreateAsync();

        CollectionModel<TransactionModel> result = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.accountIds={cash.Id}&filter.fundIds={groceries.Id}&filter.accountingPeriodIds={july.Id}&filter.types=Spending&sort=Destination");

        TransactionModel transaction = Assert.Single(result.Items);
        Assert.Equal(groceriesTransaction.Id, transaction.Id);
        Assert.Equal(1, result.TotalCount);
    }

    /// <summary>
    /// Rejects reversed accounting-period ranges on each aggregate read surface.
    /// </summary>
    [Fact]
    public async Task AccountingPeriodRangesRejectReversedEndpointsAcrossReadSurfaces()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        string range = $"range.start={august.Id}&range.end={july.Id}";

        using HttpResponseMessage accounts = await test.Api.GetResponseAsync($"/accounts/accounting-period-range?{range}");
        using HttpResponseMessage funds = await test.Api.GetResponseAsync($"/funds/accounting-period-range?{range}");
        using HttpResponseMessage transactions = await test.Api.GetResponseAsync($"/transactions/accounting-period-range?{range}");
        using HttpResponseMessage events = await test.Api.GetResponseAsync($"/fund-goals/balance-events/accounting-period-range?{range}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, accounts.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, funds.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, transactions.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, events.StatusCode);
    }

    /// <summary>
    /// Applies date sorting and limits after filtering while retaining the unpaged total count.
    /// </summary>
    [Fact]
    public async Task TransactionListSortAndLimitPreserveFilteredTotalCount()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(10m).From(cash).To("First", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 20)).For(10m).From(cash).To("Second", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 30)).For(10m).From(cash).To("Third", groceries).CreateAsync();

        CollectionModel<TransactionModel> response = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.accountIds={cash.Id}&filter.accountingPeriodIds={july.Id}&sort=DateDescending&limit=2");

        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.Items.Count);
        Assert.Equal(["Third", "Second"], response.Items.Select(item => item.Description));
    }
}
