using System.Net;
using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;

namespace Tests.AccountingPeriods;

/// <summary>
/// Covers accounting-period lists, ranges, and period-scoped transaction pages.
/// </summary>
public sealed class AccountingPeriodQueryTests
{
    /// <summary>
    /// Filters, sorts, and pages accounting-period lists with and without balances.
    /// </summary>
    [Fact]
    public async Task ListsFilterSortAndPage()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 15));

        CollectionModel<AccountingPeriodWithBalanceModel> balances = await test.Api.GetAsync<CollectionModel<AccountingPeriodWithBalanceModel>>(
            "/accounting-periods/with-balances?filter.months=7&filter.months=8&sort=ClosingBalanceDescending&offset=1&limit=1");
        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?filter.months=8&sort=DateDescending");

        Assert.Equal(2, balances.TotalCount);
        AccountingPeriodWithBalanceModel period = Assert.Single(balances.Items);
        Assert.Equal(july.Id, period.Id);
        Assert.Equal(100m, period.OpeningBalance);
        Assert.Equal(90m, period.ClosingBalance);
        Assert.Equal(august.Id, Assert.Single(periods.Items).Id);
    }

    /// <summary>
    /// Returns an account's and a fund's period detail with their totals and events, or 404 when either side is missing.
    /// </summary>
    [Fact]
    public async Task EntityDetailReturnsBalancesTotalsAndEvents()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 7, 15));

        PeriodAccountDetailModel account = await test.Api.GetAsync<PeriodAccountDetailModel>($"/accounting-periods/{july.Id}/accounts/{cash.Id}");
        PeriodFundDetailModel fund = await test.Api.GetAsync<PeriodFundDetailModel>($"/accounting-periods/{july.Id}/funds/{groceries.Id}");
        using HttpResponseMessage missingFund = await test.Api.GetResponseAsync($"/accounting-periods/{july.Id}/funds/{Guid.NewGuid()}");
        using HttpResponseMessage missingPeriod = await test.Api.GetResponseAsync($"/accounting-periods/{Guid.NewGuid()}/accounts/{cash.Id}");

        Assert.Equal(august.Id, account.NextPeriod?.Id);
        Assert.Equal(100m, account.Account.OpeningBalance);
        Assert.Equal(130m, account.Account.ClosingBalance);
        Assert.Equal(40m, account.TotalInflow);
        Assert.Equal(10m, account.TotalOutflow);
        Assert.NotEmpty(account.Events);
        Assert.Equal(30m, fund.Fund.ClosingBalance);
        Assert.Equal(40m, fund.TotalInflow);
        Assert.Equal(10m, fund.TotalOutflow);
        Assert.NotEmpty(fund.Events);
        Assert.Equal(HttpStatusCode.NotFound, missingFund.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingPeriod.StatusCode);
    }

    /// <summary>
    /// Narrows the account and fund balance lists to the requested IDs.
    /// </summary>
    [Fact]
    public async Task BalanceListsFilterByIds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        _ = await test.Accounts.Onboard("Savings").WithOpeningBalance(50m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Funds.Create("Rent").In(july).CreateAsync();

        CollectionModel<AccountWithBalanceModel> accounts = await test.Api.GetAsync<CollectionModel<AccountWithBalanceModel>>($"/accounts/with-balances?filter.ids={cash.Id}");
        CollectionModel<FundWithBalanceModel> funds = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>($"/funds/with-balances?filter.ids={groceries.Id}");

        Assert.Equal(cash.Id, Assert.Single(accounts.Items).Id);
        Assert.Equal(groceries.Id, Assert.Single(funds.Items).Id);
    }

    /// <summary>
    /// Limits both period lists to open or closed periods.
    /// </summary>
    [Fact]
    public async Task ListsFilterByIsOpen()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        await test.Periods.CloseAsync(july);

        CollectionModel<AccountingPeriodModel> open = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>("/accounting-periods?filter.isOpen=true");
        CollectionModel<AccountingPeriodModel> closed = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>("/accounting-periods?filter.isOpen=false");
        CollectionModel<AccountingPeriodWithBalanceModel> openBalances = await test.Api.GetAsync<CollectionModel<AccountingPeriodWithBalanceModel>>(
            "/accounting-periods/with-balances?filter.isOpen=true");

        Assert.Equal(august.Id, Assert.Single(open.Items).Id);
        Assert.Equal(july.Id, Assert.Single(closed.Items).Id);
        Assert.Equal(august.Id, Assert.Single(openBalances.Items).Id);
    }

    /// <summary>
    /// Returns the earliest and latest periods across years, or neither before any exist.
    /// </summary>
    [Fact]
    public async Task BoundsReturnEarliestAndLatestPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodBoundsModel empty = await test.Api.GetAsync<AccountingPeriodBoundsModel>("/accounting-periods/bounds");
        AccountingPeriodHandle december = await test.Periods.Create(2025, 12).CreateAsync();
        _ = await test.Periods.Create(2026, 1).CreateAsync();
        AccountingPeriodHandle february = await test.Periods.Create(2026, 2).CreateAsync();

        AccountingPeriodBoundsModel bounds = await test.Api.GetAsync<AccountingPeriodBoundsModel>("/accounting-periods/bounds");

        Assert.Null(empty.First);
        Assert.Null(empty.Latest);
        Assert.Equal(december.Id, bounds.First?.Id);
        Assert.Equal(february.Id, bounds.Latest?.Id);
    }

    /// <summary>
    /// Returns the periods in a range in date order, each with its own expected income.
    /// </summary>
    [Fact]
    public async Task RangeReturnsPeriodsInDateOrderWithTheirExpectedIncome()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        await test.Api.PostAsync(
            $"/accounting-periods/{july.Id}/expected-income-sources",
            new ExpectedIncomeSourceRequestModel
            {
                Name = "Employer",
                IncomeLines = [new CreateIncomeLineModel { Description = "Salary", Amount = 100m }],
                IncomeDeductions = [],
                UntrackedTransfers = [new ExpectedUntrackedIncomeTransferRequestModel { Description = "Transfer", Amount = 20m }],
                ExpectedDates = [new DateOnly(2026, 7, 15)],
            });
        await test.Api.PostAsync(
            $"/accounting-periods/{august.Id}/expected-income-sources",
            new ExpectedIncomeSourceRequestModel
            {
                Name = "Employer",
                IncomeLines = [new CreateIncomeLineModel { Description = "Salary", Amount = 100m }],
                IncomeDeductions = [],
                UntrackedTransfers = [],
                ExpectedDates = [new DateOnly(2026, 8, 15)],
            });

        AccountingPeriodsInRangeModel range = await test.Api.GetAsync<AccountingPeriodsInRangeModel>(
            $"/accounting-periods/range?range.start={july.Id}&range.end={august.Id}");

        Assert.Equal([july.Id, august.Id], range.AccountingPeriods.Select(period => period.Id));
        Assert.Equal([100m, 100m], range.AccountingPeriods.Select(period => period.ExpectedIncome.Total));
        Assert.Equal([80m, 100m], range.AccountingPeriods.Select(period => period.ExpectedIncome.Tracked));
    }

    /// <summary>
    /// Calculates each period's own income and Fund Goal totals when several periods are returned.
    /// </summary>
    [Fact]
    public async Task MultiplePeriodsEachGetTheirOwnCalculatedTotals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).WithGoal(50m).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        TransactionHandle julyIncome = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(60m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle augustIncome = await test.Transactions.Income().In(august).On(new DateOnly(2026, 8, 10)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        await test.Transactions.PostAsync(julyIncome, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(augustIncome, cash, new DateOnly(2026, 8, 10));

        CollectionModel<AccountingPeriodWithBalanceModel> balances = await test.Api.GetAsync<CollectionModel<AccountingPeriodWithBalanceModel>>(
            "/accounting-periods/with-balances?sort=Date");
        AccountingPeriodsInRangeModel range = await test.Api.GetAsync<AccountingPeriodsInRangeModel>(
            $"/accounting-periods/range?range.start={july.Id}&range.end={august.Id}");

        Assert.Equal([60m, 40m], balances.Items.Select(period => period.ActualIncome.Total));
        Assert.All(balances.Items, period => Assert.Equal(50m, period.PlannedGoalContributions));
        Assert.Equal([60m, 40m], range.AccountingPeriods.Select(period => period.ActualIncome.Total));
        Assert.All(range.AccountingPeriods, period => Assert.Equal(50m, period.PlannedGoalContributions));
    }

    /// <summary>
    /// Returns a period's totals, its neighbors, and its opening and closing balances in one response.
    /// </summary>
    [Fact]
    public async Task DetailReturnsTotalsNeighborsAndBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle june = await test.Periods.Create(2026, 6).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Funds.Create("Dining").In(july).WithGoal(50m, maximumEndingBalance: 0m).CreateAsync();
        _ = await test.Funds.Create("Travel").In(july).WithGoal(25m).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(60m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 7, 15));

        AccountingPeriodDetailModel detail = await test.Api.GetAsync<AccountingPeriodDetailModel>($"/accounting-periods/{july.Id}/detail");
        AccountingPeriodDetailModel first = await test.Api.GetAsync<AccountingPeriodDetailModel>($"/accounting-periods/{june.Id}/detail");
        HttpResponseMessage missing = await test.Api.GetResponseAsync($"/accounting-periods/{Guid.NewGuid()}/detail");

        Assert.Equal(july.Id, detail.AccountingPeriod.Id);
        Assert.NotEmpty(detail.Accounts);
        Assert.NotEmpty(detail.Funds);
        Assert.All(detail.Funds, fund => Assert.NotNull(fund.GoalProgress));
        Assert.Equal(june.Id, detail.PreviousPeriod?.Id);
        Assert.Equal(august.Id, detail.NextPeriod?.Id);
        Assert.Equal(60m, detail.AccountingPeriod.ActualIncome.Total);
        Assert.Equal(20m, detail.TotalSpending);
        Assert.Equal(75m, detail.AccountingPeriod.PlannedGoalContributions);
        Assert.Equal(25m, detail.AccountingPeriod.ExpectedGoalContributions);
        Assert.Equal(100m, detail.AccountOpeningBalance.TotalBalance);
        Assert.Equal(140m, detail.AccountClosingBalance.TotalBalance);
        Assert.Equal(100m, detail.FundOpeningBalance.TotalBalance);
        Assert.Equal(40m, detail.FundClosingBalance.TotalAssignedBalance);
        Assert.Null(first.PreviousPeriod);
        Assert.Equal(july.Id, first.NextPeriod?.Id);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Accepts a contiguous accounting-period range and rejects missing or reversed endpoints on every range surface.
    /// </summary>
    [Fact]
    public async Task RangesRejectMissingAndReversedEndpoints()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        string reversed = $"range.start={august.Id}&range.end={july.Id}";

        TransactionsInAccountingPeriodRangeModel transactions = await test.Api.GetAsync<TransactionsInAccountingPeriodRangeModel>(
            $"/transactions/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync($"/accounting-periods/range?range.start={july.Id}&range.end={Guid.NewGuid()}");
        using HttpResponseMessage accounts = await test.Api.GetResponseAsync($"/accounts/accounting-period-range?{reversed}");
        using HttpResponseMessage funds = await test.Api.GetResponseAsync($"/funds/accounting-period-range?{reversed}");
        using HttpResponseMessage reversedTransactions = await test.Api.GetResponseAsync($"/transactions/accounting-period-range?{reversed}");

        Assert.Empty(transactions.Transactions.Items);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missing.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, accounts.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, funds.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reversedTransactions.StatusCode);
    }

    /// <summary>
    /// Returns an Expected Income Source with its linked income in date order, counting only posted income as received.
    /// </summary>
    [Fact]
    public async Task ExpectedIncomeSourceReturnsItsLinkedIncome()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        Guid employer = await test.Api.AddExpectedIncomeSourceAsync(july, "Employer", 100m, new DateOnly(2026, 7, 15));
        TransactionHandle second = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 20)).For(40m).From("Employer").PlannedAs(employer).To(cash, groceries).CreateAsync();
        TransactionHandle first = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(100m).From("Employer").PlannedAs(employer).To(cash, groceries).CreateAsync();
        _ = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 16)).For(25m).From("Gift").To(cash, groceries).CreateAsync();
        await test.Transactions.PostAsync(first, cash, new DateOnly(2026, 7, 15));

        ExpectedIncomeSourceDetailModel detail = await test.Api.GetAsync<ExpectedIncomeSourceDetailModel>(
            $"/accounting-periods/{july.Id}/expected-income-sources/{employer}");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync(
            $"/accounting-periods/{july.Id}/expected-income-sources/{Guid.NewGuid()}");

        Assert.Equal(july.Id, detail.AccountingPeriod.Id);
        Assert.Equal("Employer", detail.Source.Name);
        Assert.Equal(100m, detail.Source.ReceivedAmount.Total);
        Assert.Equal(100m, detail.Source.ReceivedAmount.Tracked);
        Assert.Equal([first.Id, second.Id], detail.Income.Select(income => income.TransactionId));
        Assert.Equal([100m, 40m], detail.Income.Select(income => income.Amount));
        Assert.Equal([100m, 0m], detail.Income.Select(income => income.ReceivedAmount.Total));
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Returns the requested periods in date order with their sources and received income, leaving out unknown periods.
    /// </summary>
    [Fact]
    public async Task ExpectedIncomeSourcesAreReturnedForRequestedPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        Guid employer = await test.Api.AddExpectedIncomeSourceAsync(july, "Employer", 100m, new DateOnly(2026, 7, 15));
        _ = await test.Api.AddExpectedIncomeSourceAsync(august, "Side job", 50m, new DateOnly(2026, 8, 15));
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(100m).From("Employer").PlannedAs(employer).To(cash, groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 15));

        IReadOnlyCollection<AccountingPeriodWithExpectedIncomeSourcesModel> periods =
            await test.Api.GetAsync<IReadOnlyCollection<AccountingPeriodWithExpectedIncomeSourcesModel>>(
                $"/accounting-periods/expected-income-sources?accountingPeriodIds={august.Id}&accountingPeriodIds={july.Id}&accountingPeriodIds={Guid.NewGuid()}");

        Assert.Equal([july.Id, august.Id], periods.Select(period => period.Id));
        ExpectedIncomeSourceModel employerSource = Assert.Single(periods.First().ExpectedIncomeSources);
        Assert.Equal("Employer", employerSource.Name);
        Assert.Equal(100m, employerSource.ReceivedAmount.Tracked);
        ExpectedIncomeSourceModel sideJob = Assert.Single(periods.Last().ExpectedIncomeSources);
        Assert.Equal("Side job", sideJob.Name);
        Assert.Equal(0m, sideJob.ReceivedAmount.Total);
    }

    /// <summary>
    /// An all-zero ID can't name anything, so every route reports it as not found instead of failing.
    /// </summary>
    [Theory]
    [InlineData("/accounting-periods/00000000-0000-0000-0000-000000000000")]
    [InlineData("/accounting-periods/00000000-0000-0000-0000-000000000000/detail")]
    [InlineData("/accounts/with-balances?filter.ids=00000000-0000-0000-0000-000000000000")]
    [InlineData("/funds/with-balances?filter.ids=00000000-0000-0000-0000-000000000000")]
    public async Task EmptyEntityIdsAreNotFound(string route)
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();

        using HttpResponseMessage response = await test.Api.GetResponseAsync(route);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// An all-zero Account or Fund in a real Accounting Period is not found.
    /// </summary>
    [Fact]
    public async Task EmptyEntityIdsInAPeriodAreNotFound()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage account = await test.Api.GetResponseAsync($"/accounting-periods/{july.Id}/accounts/{Guid.Empty}");
        using HttpResponseMessage fund = await test.Api.GetResponseAsync($"/accounting-periods/{july.Id}/funds/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.NotFound, account.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, fund.StatusCode);
    }
}
