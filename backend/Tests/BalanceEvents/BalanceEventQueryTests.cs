using System.Net;
using Models;
using Models.Accounts;
using Models.BalanceEvents;
using Models.FundGoals;
using Models.Funds;
using Models.Locations;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;

namespace Tests.BalanceEvents;

/// <summary>
/// Covers the account, fund, and fund-goal balance-event lists and totals that transactions project into.
/// </summary>
public sealed class BalanceEventQueryTests
{
    /// <summary>
    /// Pending events are opt-in, expose their counterparties without balances, and stay out of posted totals and chart points.
    /// </summary>
    [Fact]
    public async Task PendingEventsAreOptInAndExcludedFromPostedSummaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();

        CollectionModel<AccountBalanceEventModel> defaultAccountEvents = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31");
        CollectionModel<AccountBalanceEventModel> accountEvents = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31&includePending=true");
        CollectionModel<AccountBalanceEventModel> defaultPeriodAccountEvents = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?accountingPeriodId={july.Id}");
        CollectionModel<AccountBalanceEventModel> periodAccountEvents = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?accountingPeriodId={july.Id}&includePending=true");
        CollectionModel<FundBalanceEventModel> defaultFundEvents = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31&filter.names=Groceries");
        CollectionModel<FundBalanceEventModel> fundEvents = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31&filter.names=Groceries&includePending=true");
        CollectionModel<FundGoalBalanceEventModel> goalEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}");
        BalanceEventTotalsModel accountTotals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            $"/accounts/{cash.Id}/balance-events/totals?range.start=2026-07-01&range.end=2026-07-31&includePending=true");
        BalanceEventTotalsModel fundTotals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            "/funds/balance-events/date-range/totals?range.start=2026-07-01&range.end=2026-07-31&filter.names=Groceries&includePending=true");

        Assert.Empty(defaultAccountEvents.Items);
        Assert.Empty(defaultPeriodAccountEvents.Items);
        Assert.Empty(defaultFundEvents.Items);
        AccountBalanceEventModel accountEvent = Assert.Single(accountEvents.Items);
        Assert.Equal(transaction.Id, accountEvent.TransactionId);
        Assert.Equal(BalanceEventTypeModel.Debit, accountEvent.Type);
        Assert.Equal("Cash", accountEvent.Source.DisplayName);
        Assert.Equal("Market", Assert.Single(accountEvent.Destinations).DisplayName);
        Assert.False(accountEvent.IsPosted);
        Assert.Null(accountEvent.EventDate);
        Assert.Null(accountEvent.PreviousBalance);
        Assert.Null(accountEvent.NewBalance);
        Assert.Equal(transaction.Id, Assert.Single(periodAccountEvents.Items).TransactionId);
        FundBalanceEventModel fundEvent = Assert.Single(fundEvents.Items);
        Assert.Equal(transaction.Id, fundEvent.TransactionId);
        Assert.False(fundEvent.IsPosted);
        Assert.Null(fundEvent.EventDate);
        FundGoalBalanceEventModel goalEvent = Assert.Single(goalEvents.Items, item => item.TransactionId == transaction.Id);
        Assert.False(goalEvent.IsPosted);
        Assert.Equal("Cash", goalEvent.Source.DisplayName);
        Assert.Equal("Market", Assert.Single(goalEvent.Destinations).DisplayName);
        Assert.Null(goalEvent.NewTotals);
        Assert.Equal(0m, accountTotals.TotalOutflow);
        Assert.Equal(0m, fundTotals.TotalOutflow);
        Assert.All(accountTotals.Dates!, day => Assert.Equal(100m, day.TotalBalance));
        Assert.All(fundTotals.Dates!, day => Assert.Equal(0m, day.TotalBalance));
    }

    /// <summary>
    /// Selects balance events and chart points by posting date rather than transaction date.
    /// </summary>
    [Fact]
    public async Task DateRangeTotalsChartUsesPostingDates()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Store", groceries).CreateAsync();
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

    /// <summary>
    /// Each assignment to one fund keeps its own account posting date.
    /// </summary>
    [Fact]
    public async Task FundDateRangesSeparateAssignmentsPostedOnDifferentDates()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(30m)
            .From("Employer").To(cash, groceries, 10m).To(savings, groceries, 20m).CreateAsync();
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

    /// <summary>
    /// Fund chart checkpoints sum every selected fund, including funds without a posting that day.
    /// </summary>
    [Fact]
    public async Task FundTotalsChartCombinesSelectedFunds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle travel = await test.Funds.Create("Travel").In(july).CreateAsync();
        TransactionHandle groceryPurchase = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 1)).For(10m).From(cash).To("Store", groceries).CreateAsync();
        TransactionHandle travelPurchase = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 2)).For(20m).From(cash).To("Train", travel).CreateAsync();
        await test.Transactions.PostAsync(groceryPurchase, cash, new DateOnly(2026, 7, 1));
        await test.Transactions.PostAsync(travelPurchase, cash, new DateOnly(2026, 7, 2));

        BalanceEventTotalsModel totals = await test.Api.GetAsync<BalanceEventTotalsModel>(
            "/funds/balance-events/date-range/totals?range.start=2026-07-01&range.end=2026-07-03&filter.names=Groceries&filter.names=Travel");

        Assert.Equal(30m, totals.TotalOutflow);
        Assert.Equal(0m, totals.OpeningBalance);
        Assert.Equal([-10m, -30m, -30m], totals.Dates!.Select(day => day.TotalBalance));
    }

    /// <summary>
    /// Keeps pending events from other periods out of an account's period-scoped event list.
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
        _ = await test.Transactions.Spending().In(august).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("August", augustFund).CreateAsync();

        CollectionModel<AccountBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?accountingPeriodId={july.Id}&includePending=true");

        Assert.Equal(1, events.TotalCount);
        Assert.Equal(julyTransaction.Id, Assert.Single(events.Items).TransactionId);
    }

    /// <summary>
    /// Orders and pages posted events consistently across the account, fund, and fund-goal surfaces.
    /// </summary>
    [Fact]
    public async Task QueriesOrderAndPageAcrossSurfaces()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle earlier = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(10m).From(cash).To("First", groceries).CreateAsync();
        TransactionHandle later = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 20)).For(20m).From(cash).To("Second", groceries).CreateAsync();
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 20));
        string accountUri = $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31";
        const string fundUri = "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31";
        string goalUri = $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}";

        CollectionModel<AccountBalanceEventModel> accounts = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>($"{accountUri}&sort=Date");
        CollectionModel<FundBalanceEventModel> funds = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>($"{fundUri}&sort=Date");
        CollectionModel<FundGoalBalanceEventModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>($"{goalUri}&sort=Date");
        Assert.Equal([earlier.Id, later.Id], accounts.Items.Select(item => item.TransactionId));
        Assert.Equal([earlier.Id, later.Id], funds.Items.Select(item => item.TransactionId));
        Assert.Equal([earlier.Id, later.Id], goals.Items.Select(item => item.TransactionId));
        Assert.All(accounts.Items, item => Assert.True(item.IsPosted));
        Assert.All(funds.Items, item => Assert.True(item.IsPosted));
        Assert.All(goals.Items, item => Assert.True(item.IsPosted));

        accounts = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>($"{accountUri}&sort=DateDescending&limit=1");
        funds = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>($"{fundUri}&sort=DateDescending&limit=1");
        goals = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>($"{goalUri}&sort=DateDescending&limit=1");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync(
            $"/accounts/{Guid.NewGuid()}/balance-events?range.start=2026-07-01&range.end=2026-07-31");
        Assert.Equal(2, accounts.TotalCount);
        Assert.Equal(later.Id, Assert.Single(accounts.Items).TransactionId);
        Assert.Equal(2, funds.TotalCount);
        Assert.Equal(later.Id, Assert.Single(funds.Items).TransactionId);
        Assert.Equal(2, goals.TotalCount);
        Assert.Equal(later.Id, Assert.Single(goals.Items).TransactionId);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Fund-goal spending events use the source account's posting date and carry prior posted spending totals.
    /// </summary>
    [Fact]
    public async Task FundGoalSpendingEventsUsePostingDateAndCarryPriorTotals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle earlier = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(25m).From(cash).To("Market", groceries).CreateAsync();
        TransactionHandle later = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(15m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 16));

        CollectionModel<FundGoalBalanceEventModel> goalEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}");

        FundGoalBalanceEventModel laterEvent = Assert.Single(goalEvents.Items, item => item.TransactionId == later.Id);
        Assert.True(laterEvent.IsPosted);
        Assert.Equal(new DateOnly(2026, 7, 16), laterEvent.EventDate);
        Assert.Equal(25m, laterEvent.PreviousTotals?.AmountSpent);
        Assert.Equal(40m, laterEvent.NewTotals?.AmountSpent);
    }

    /// <summary>
    /// Projects a fund transfer with its counterparty funds on both fund surfaces.
    /// </summary>
    [Fact]
    public async Task FundTransferEventsExposeFundCounterparties()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From(groceries).To(dining).CreateAsync();

        CollectionModel<FundBalanceEventModel> fundEvents = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31");
        CollectionModel<FundGoalBalanceEventModel> goalEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}");

        FundBalanceEventModel fund = Assert.Single(fundEvents.Items, item => item.TransactionId == transaction.Id && item.Fund.Id == groceries.Id);
        FundGoalBalanceEventModel goal = Assert.Single(goalEvents.Items, item => item.TransactionId == transaction.Id && item.Fund.Id == groceries.Id);
        Assert.Equal("Groceries", fund.Source.DisplayName);
        Assert.Equal("Dining", Assert.Single(fund.Destinations).DisplayName);
        Assert.Equal("Groceries", goal.Source.DisplayName);
        Assert.Equal("Dining", Assert.Single(goal.Destinations).DisplayName);
    }

    /// <summary>
    /// Includes automatically assigned Unassigned income in pending and posted fund-goal events and transaction details.
    /// </summary>
    [Fact]
    public async Task AutomaticIncomeRemainderProjectsToUnassignedFundGoal()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle unassigned = await test.FundQueries.GetUnassignedAsync(july);
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Pay",
            Amount = 100m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = new LocationInputModel { NewLocationName = "Employer" },
                IncomeLines = [new CreateIncomeLineModel { Description = "Pay", Amount = 100m }],
                IncomeDeductions = [],
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 100m,
                FundAssignments = [new CreateIncomeFundAmountModel { FundId = groceries.Id, Amount = 40m }],
            }],
        });
        string goalEventsUri = $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}";

        FundGoalBalanceEventModel pending = Assert.Single((await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(goalEventsUri)).Items,
            item => item.TransactionId == created.Id && item.Fund.Id == unassigned.Id);
        IncomeTransactionModel pendingDetail = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{created.Id}");
        await test.Transactions.PostAsync(new TransactionHandle(created.Id), cash, new DateOnly(2026, 7, 15));
        FundGoalBalanceEventModel posted = Assert.Single((await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(goalEventsUri)).Items,
            item => item.TransactionId == created.Id && item.Fund.Id == unassigned.Id);
        IncomeTransactionModel postedDetail = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{created.Id}");

        Assert.Null(pending.NewTotals);
        Assert.False(pending.IsPosted);
        Assert.Contains(pendingDetail.Destinations.Single().FundGoals, goal => goal.Fund.Id == unassigned.Id);
        Assert.Equal(60m, posted.NewTotals?.AmountAssigned);
        Assert.True(posted.IsPosted);
        Assert.Contains(postedDetail.Destinations.Single().FundGoals, goal => goal.Fund.Id == unassigned.Id);
    }
}
