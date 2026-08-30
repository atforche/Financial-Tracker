using Models;
using Models.Accounts;
using Models.BalanceEvents;
using Models.FundGoals;
using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Characterizes the API's balance-event interpretation for each financial surface.
/// </summary>
public sealed class BalanceEventProjectionTests
{
    /// <summary>
    /// Includes automatically assigned Unassigned income in pending and posted
    /// Fund Goal events and transaction details.
    /// </summary>
    [Fact]
    public async Task AutomaticIncomeRemainderProjectsToUnassignedFundGoal()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        CollectionModel<FundModel> funds = await test.Api.GetAsync<CollectionModel<FundModel>>("/funds");
        FundModel unassigned = Assert.Single(funds.Items, fund => fund.Name == "Unassigned");
        FundGoalModel unassignedGoal = await test.Api.GetAsync<FundGoalModel>(
            $"/fund-goals/fund/{unassigned.Id}?accountingPeriodId={july.Id}");

        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>(
            "/transactions",
            new CreateIncomeTransactionModel
            {
                AccountingPeriodId = july.Id,
                Date = new DateOnly(2026, 7, 15),
                Description = "Pay",
                Amount = 100m,
                Source = new CreateIncomeTransactionSourceModel
                {
                    Location = new Models.Locations.LocationInputModel { NewLocationName = "Employer" },
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

        CollectionModel<FundGoalBalanceEventModel> pendingEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}");
        IncomeTransactionModel pendingDetail = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{created.Id}");
        FundGoalBalanceEventModel pending = Assert.Single(pendingEvents.Items,
            item => item.TransactionId == created.Id && item.Fund.Id == unassigned.Id);

        await test.Transactions.PostAsync(new TransactionHandle(created.Id), cash, new DateOnly(2026, 7, 15));

        CollectionModel<FundGoalBalanceEventModel> postedEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}");
        FundGoalBalanceEventModel posted = Assert.Single(postedEvents.Items,
            item => item.TransactionId == created.Id && item.Fund.Id == unassigned.Id);
        IncomeTransactionModel postedDetail = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{created.Id}");

        Assert.Equal(60m, pending.NewTotals.AmountAssignedIncludingPending);
        Assert.False(pending.IsPosted);
        Assert.Contains(pendingDetail.Destinations.Single().FundGoals, goal => goal.Fund.Id == unassigned.Id);
        Assert.Equal(60m, posted.NewTotals.AmountAssigned);
        Assert.True(posted.IsPosted);
        Assert.Contains(postedDetail.Destinations.Single().FundGoals, goal => goal.Fund.Id == unassigned.Id);
        Assert.Equal(unassignedGoal.Fund.Id, postedDetail.Destinations.Single().FundGoals.Single(goal => goal.Fund.Id == unassigned.Id).Fund.Id);
    }

    /// <summary>
    /// Projects a spending transaction with the account and location counterparties expected by each surface.
    /// </summary>
    [Fact]
    public async Task SpendingEventsExposeDirectionCounterpartiesAndPendingBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From(cash).To("Market", groceries).CreateAsync();

        CollectionModel<AccountBalanceEventModel> accountEvents = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31");
        CollectionModel<FundBalanceEventModel> fundEvents = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31");
        CollectionModel<FundGoalBalanceEventModel> goalEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            "/fund-goals/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31");

        AccountBalanceEventModel account = Assert.Single(accountEvents.Items, item => item.TransactionId == transaction.Id);
        FundBalanceEventModel fund = Assert.Single(fundEvents.Items, item => item.TransactionId == transaction.Id);
        FundGoalBalanceEventModel goal = Assert.Single(goalEvents.Items, item => item.TransactionId == transaction.Id);

        Assert.Equal(BalanceEventTypeModel.Debit, account.Type);
        Assert.Equal("Cash", account.Source.DisplayName);
        Assert.Equal("Market", Assert.Single(account.Destinations).DisplayName);
        Assert.False(account.IsPosted);
        Assert.Equal(100m, account.PreviousBalance.PostedBalance);
        Assert.Equal(75m, account.NewBalance.BalanceIncludingPending);

        Assert.Equal(BalanceEventTypeModel.Debit, fund.Type);
        Assert.Equal("Cash", fund.Source.DisplayName);
        Assert.Equal("Market", Assert.Single(fund.Destinations).DisplayName);
        Assert.Equal(-25m, fund.NewBalance.BalanceIncludingPending);
        Assert.Equal("Cash", goal.Source.DisplayName);
        Assert.Equal("Market", Assert.Single(goal.Destinations).DisplayName);
        Assert.Equal(25m, goal.NewTotals.AmountSpentIncludingPending);
    }

    /// <summary>
    /// Uses the spending source Account's posting date for Fund Goal spending events.
    /// </summary>
    [Fact]
    public async Task PostedSpendingEventsArePostedForFundGoals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));

        CollectionModel<FundGoalBalanceEventModel> goalEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}");

        FundGoalBalanceEventModel goal = Assert.Single(goalEvents.Items, item => item.TransactionId == transaction.Id);
        Assert.True(goal.IsPosted);
        Assert.Equal(new DateOnly(2026, 7, 16), goal.EventDate);
    }

    /// <summary>
    /// Carries prior posted spending into later Fund Goal balance event totals.
    /// </summary>
    [Fact]
    public async Task FundGoalEventsPreservePriorPostedSpendingTotals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle earlier = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(25m).From(cash).To("Market", groceries).CreateAsync();
        TransactionHandle later = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(15m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 15));

        CollectionModel<FundGoalBalanceEventModel> goalEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}");

        FundGoalBalanceEventModel laterEvent = Assert.Single(goalEvents.Items, item => item.TransactionId == later.Id);
        Assert.Equal(25m, laterEvent.PreviousTotals.AmountSpent);
        Assert.Equal(40m, laterEvent.NewTotals.AmountSpent);
    }

    /// <summary>
    /// Projects a fund transfer with its counterparty Funds on both balance surfaces.
    /// </summary>
    [Fact]
    public async Task FundTransferEventsExposeFundAndAccountCounterparties()
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
            "/fund-goals/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31");

        FundBalanceEventModel fund = Assert.Single(fundEvents.Items, item => item.TransactionId == transaction.Id && item.Fund.Id == groceries.Id);
        FundGoalBalanceEventModel goal = Assert.Single(goalEvents.Items, item => item.TransactionId == transaction.Id && item.Fund.Id == groceries.Id);

        Assert.Equal("Groceries", fund.Source.DisplayName);
        Assert.Equal("Dining", Assert.Single(fund.Destinations).DisplayName);
        Assert.Equal("Groceries", goal.Source.DisplayName);
        Assert.Equal("Dining", Assert.Single(goal.Destinations).DisplayName);
    }

    /// <summary>
    /// Keeps posted income event state and date ordering aligned across account, Fund, and Fund Goal projections.
    /// </summary>
    [Fact]
    public async Task PostedIncomeEventsAreProjectedAcrossAllBalanceSurfacesInDateOrder()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        TransactionHandle earlier = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(20m).From("Employer").To(cash, income).CreateAsync();
        TransactionHandle later = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 20)).For(30m).From("Employer").To(cash, income).CreateAsync();
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 20));

        CollectionModel<AccountBalanceEventModel> accountEvents = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31&sort=Date");
        CollectionModel<FundBalanceEventModel> fundEvents = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            $"/funds/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}&sort=Date");
        CollectionModel<FundGoalBalanceEventModel> goalEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            $"/fund-goals/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}&sort=Date");

        AccountBalanceEventModel[] accountIncomeEvents = accountEvents.Items.Where(item => item.TransactionId == earlier.Id || item.TransactionId == later.Id).ToArray();
        FundBalanceEventModel[] fundIncomeEvents = fundEvents.Items.Where(item => item.TransactionId == earlier.Id || item.TransactionId == later.Id).ToArray();
        FundGoalBalanceEventModel[] goalIncomeEvents = goalEvents.Items.Where(item => item.TransactionId == earlier.Id || item.TransactionId == later.Id).ToArray();

        Assert.Equal([earlier.Id, later.Id], accountIncomeEvents.Select(item => item.TransactionId));
        Assert.All(accountIncomeEvents, item => Assert.True(item.IsPosted));
        Assert.Equal([earlier.Id, later.Id], fundIncomeEvents.Select(item => item.TransactionId));
        Assert.All(fundIncomeEvents, item => Assert.True(item.IsPosted));
        Assert.Equal([earlier.Id, later.Id], goalIncomeEvents.Select(item => item.TransactionId));
        Assert.All(goalIncomeEvents, item => Assert.True(item.IsPosted));
    }
}
