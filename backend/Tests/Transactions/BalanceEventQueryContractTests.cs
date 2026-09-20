using System.Net;
using Models;
using Models.Accounts;
using Models.BalanceEvents;
using Models.FundGoals;
using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers balance-event paging, ordering, and missing-resource behavior.
/// </summary>
public sealed class BalanceEventQueryContractTests
{
    /// <summary>Each assignment to one Fund keeps its own Account posting date.</summary>
    [Fact]
    public async Task FundDateRangesSeparateAssignmentsPostedOnDifferentDates()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>(
            "/transactions", new CreateIncomeTransactionModel
            {
                AccountingPeriodId = july.Id,
                Date = new DateOnly(2026, 7, 15),
                Description = "Paycheck",
                Amount = 30m,
                Source = new CreateIncomeTransactionSourceModel
                {
                    Location = new Models.Locations.LocationInputModel { NewLocationName = "Employer" },
                    IncomeLines = [new CreateIncomeLineModel { Description = "Paycheck", Amount = 30m }],
                    IncomeDeductions = []
                },
                Destinations =
                [
                    new CreateIncomeTransactionDestinationModel
                    {
                        AccountId = cash.Id, Amount = 10m,
                        FundAssignments = [new CreateIncomeFundAmountModel { FundId = groceries.Id, Amount = 10m }]
                    },
                    new CreateIncomeTransactionDestinationModel
                    {
                        AccountId = savings.Id, Amount = 20m,
                        FundAssignments = [new CreateIncomeFundAmountModel { FundId = groceries.Id, Amount = 20m }]
                    }
                ]
            });
        TransactionHandle transaction = new(created.Id);
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 20));
        await test.Transactions.PostAsync(transaction, savings, new DateOnly(2026, 8, 2));

        CollectionModel<FundBalanceEventModel> julyEvents = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31&filter.names=Groceries");
        CollectionModel<FundBalanceEventModel> augustEvents = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-08-01&range.end=2026-08-31&filter.names=Groceries");
        BalanceEventTotalsModel augustTotals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            "/funds/balance-events/date-range/totals?range.start=2026-08-01&range.end=2026-08-31&filter.names=Groceries");

        Assert.Equal(10m, Assert.Single(julyEvents.Items).Amount);
        FundBalanceEventModel augustEvent = Assert.Single(augustEvents.Items);
        Assert.Equal(20m, augustEvent.Amount);
        Assert.Equal(new DateOnly(2026, 8, 2), augustEvent.EventDate);
        Assert.Equal(20m, augustTotals.TotalInflow);
    }

    /// <summary>Selects balance events by posting date rather than transaction date.</summary>
    [Fact]
    public async Task DateRangeTotalsChartUsesPostingDates()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july)
            .On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Store", groceries).CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 8, 2));

        BalanceEventTotalsModel totals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            $"/accounts/{cash.Id}/balance-events/totals?range.start=2026-07-01&range.end=2026-07-31");
        BalanceEventTotalsModel fundTotals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            "/funds/balance-events/date-range/totals?range.start=2026-07-01&range.end=2026-07-31&filter.names=Groceries");
        CollectionModel<AccountBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31");

        Assert.Empty(events.Items);
        Assert.Equal(0m, totals.TotalOutflow);
        Assert.Equal(31, totals.Dates!.Count);
        Assert.All(totals.Dates, day => Assert.Equal(100m, day.TotalBalance));
        Assert.Equal(0m, fundTotals.TotalOutflow);
        Assert.Equal(31, fundTotals.Dates!.Count);
        Assert.All(fundTotals.Dates, day => Assert.Equal(0m, day.TotalBalance));

        totals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            $"/accounts/{cash.Id}/balance-events/totals?range.start=2026-08-01&range.end=2026-08-31");
        fundTotals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            "/funds/balance-events/date-range/totals?range.start=2026-08-01&range.end=2026-08-31&filter.names=Groceries");
        events = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-08-01&range.end=2026-08-31");

        Assert.Equal(transaction.Id, Assert.Single(events.Items).TransactionId);
        Assert.Equal(10m, totals.TotalOutflow);
        Assert.Equal(90m, totals.Dates!.Single(day => day.Date == new DateOnly(2026, 8, 2)).TotalBalance);
        Assert.Equal(10m, fundTotals.TotalOutflow);
        Assert.Equal(-10m, fundTotals.Dates!.Single(day => day.Date == new DateOnly(2026, 8, 2)).TotalBalance);
    }

    /// <summary>Fund chart checkpoints sum every selected fund, including funds without a posting that day.</summary>
    [Fact]
    public async Task FundTotalsChartCombinesSelectedFunds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle travel = await test.Funds.Create("Travel").In(july).CreateAsync();
        TransactionHandle groceryPurchase = await test.Transactions.Spending().In(july)
            .On(new DateOnly(2026, 7, 1)).For(10m).From(cash).To("Store", groceries).CreateAsync();
        TransactionHandle travelPurchase = await test.Transactions.Spending().In(july)
            .On(new DateOnly(2026, 7, 2)).For(20m).From(cash).To("Train", travel).CreateAsync();
        await test.Transactions.PostAsync(groceryPurchase, cash, new DateOnly(2026, 7, 1));
        await test.Transactions.PostAsync(travelPurchase, cash, new DateOnly(2026, 7, 2));

        BalanceEventTotalsModel totals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            "/funds/balance-events/date-range/totals?range.start=2026-07-01&range.end=2026-07-03&filter.names=Groceries&filter.names=Travel");

        Assert.Equal(30m, totals.TotalOutflow);
        Assert.Equal(0m, totals.OpeningBalance);
        Assert.Equal([-10m, -30m, -30m], totals.Dates!.Select(day => day.TotalBalance));
    }

    /// <summary>
    /// Keeps pending events from other periods out of an Account Goal's event list.
    /// </summary>
    [Fact]
    public async Task AccountBalanceEventsCanBeRestrictedToAnAccountingPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle julyFund = await test.Funds.Create("July Fund").In(july).CreateAsync();
        FundHandle augustFund = await test.Funds.Create("August Fund").In(august).CreateAsync();
        TransactionHandle julyTransaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 8, 15)).For(10m).From(cash).To("July", julyFund).CreateAsync();
        TransactionHandle augustTransaction = await test.Transactions.Spending().In(august).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("August", augustFund).CreateAsync();

        CollectionModel<AccountBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?accountingPeriodId={july.Id}");

        Assert.Equal(1, events.TotalCount);
        Assert.Equal(julyTransaction.Id, Assert.Single(events.Items).TransactionId);
        Assert.DoesNotContain(events.Items, item => item.TransactionId == augustTransaction.Id);
    }

    /// <summary>
    /// Applies descending ordering and limits consistently across each balance-event surface.
    /// </summary>
    [Fact]
    public async Task BalanceEventQueriesOrderAndPageAcrossSurfaces()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash")
            .WithFinancialInstitution("Ally")
            .WithOpeningBalance(100m)
            .CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle earlier = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(10m).From(cash).To("First", groceries).CreateAsync();
        TransactionHandle later = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 20)).For(20m).From(cash).To("Second", groceries).CreateAsync();
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 20));

        CollectionModel<AccountBalanceEventModel> accounts = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31&sort=DateDescending&limit=1");
        CollectionModel<FundBalanceEventModel> funds = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31&sort=DateDescending&limit=1");
        CollectionModel<FundGoalBalanceEventModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}&sort=DateDescending&limit=1");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync(
            $"/accounts/{Guid.NewGuid()}/balance-events?range.start=2026-07-01&range.end=2026-07-31");

        Assert.Equal(2, accounts.TotalCount);
        Assert.Equal(later.Id, Assert.Single(accounts.Items).TransactionId);
        Assert.Equal(2, funds.TotalCount);
        Assert.Equal(later.Id, Assert.Single(funds.Items).TransactionId);
        Assert.Equal(2, goals.TotalCount);
        Assert.Equal(later.Id, Assert.Single(goals.Items).TransactionId);
        Assert.DoesNotContain(accounts.Items, item => item.TransactionId == earlier.Id);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }
}
