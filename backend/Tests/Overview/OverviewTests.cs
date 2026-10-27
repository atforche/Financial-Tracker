using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.Funds;
using Models.Overview;
using Models.Transactions.Create;

namespace Tests.Overview;

/// <summary>
/// Covers the balances, current Accounting Period, and items needing attention returned for the overview.
/// </summary>
public sealed class OverviewTests
{
    /// <summary>
    /// Matches the Account and Fund date-range summaries over the history range ending on the as-of date.
    /// </summary>
    [Fact]
    public async Task HistoryMatchesAccountAndFundSummaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle card = await test.Accounts.Create("Card", july).WithType(AccountTypeModel.CreditCard).OpenedOn(new DateOnly(2026, 7, 15)).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 20)).For(15m).From(card).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(spending, card, new DateOnly(2026, 7, 20));

        OverviewModel overview = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-31&history=ThreeMonths");
        const string range = "range.start=2026-04-30&range.end=2026-07-31";
        AccountsInDateRangeModel accounts = await test.Api.GetAsync<AccountsInDateRangeModel>($"/accounts/date-range?{range}");
        FundsInDateRangeModel funds = await test.Api.GetAsync<FundsInDateRangeModel>($"/funds/date-range?{range}");

        Assert.Equal(BalanceGranularityModel.Day, overview.Granularity);
        Assert.Equal(125m, overview.AccountClosingBalance.TotalBalance);
        Assert.Equal(25m, overview.FundClosingBalance.TotalAssignedBalance);
        Assert.Equal(100m, overview.FundClosingBalance.TotalUnassignedBalance);
        Assert.Equivalent(accounts.OpeningBalance, overview.AccountOpeningBalance, strict: true);
        Assert.Equal(accounts.Dates.Select(item => (item.Date, item.TotalBalance)),
            overview.AccountBalances.Select(item => (item.Date, item.TotalBalance)));
        Assert.Equal(funds.Dates.Select(item => (item.Date, item.TotalBalance)),
            overview.FundBalances.Select(item => (item.Date, item.TotalBalance)));
    }

    /// <summary>
    /// Widens the history spacing for the longer history ranges.
    /// </summary>
    [Fact]
    public async Task LongerHistoryRangesUseCoarserSpacing()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();

        OverviewModel oneYear = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-31&history=OneYear");
        OverviewModel fiveYears = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-31&history=FiveYears");

        Assert.Equal(BalanceGranularityModel.Week, oneYear.Granularity);
        Assert.Equal(BalanceGranularityModel.Month, fiveYears.Granularity);
        Assert.Equal(new DateOnly(2026, 7, 31), fiveYears.AccountBalances.Last().Date);
    }

    /// <summary>
    /// Reports the most recent open Accounting Period with its income, contribution, and spending figures.
    /// </summary>
    [Fact]
    public async Task CurrentPeriodIsTheMostRecentOpenPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        OverviewModel withoutPeriods = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-31&history=ThreeMonths");
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(august).CreateAsync();
        await AddExpectedIncomeAsync(test, august, "Employer", 100m, new DateOnly(2026, 8, 15));
        TransactionHandle income = await test.Transactions.Income().In(august).On(new DateOnly(2026, 8, 15)).For(100m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(august).On(new DateOnly(2026, 8, 16)).For(30m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 8, 15));
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 8, 16));

        OverviewModel overview = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-08-20&history=ThreeMonths");

        Assert.Null(withoutPeriods.CurrentPeriod);
        Assert.Empty(withoutPeriods.Attention);
        OverviewPeriodModel current = Assert.IsType<OverviewPeriodModel>(overview.CurrentPeriod);
        Assert.Equal(august.Id, current.AccountingPeriod.Id);
        Assert.NotEqual(july.Id, current.AccountingPeriod.Id);
        Assert.Equal(100m, current.ExpectedIncome);
        Assert.Equal(100m, current.ActualIncome);
        Assert.Equal(30m, current.TotalSpending);
    }

    /// <summary>
    /// Flags every unposted Transaction, totalled by Account as its effect on net worth.
    /// </summary>
    [Fact]
    public async Task UnpostedTransactionsAreAlwaysFlagged()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 12)).For(15m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 14)).For(5m).From(cash).To("Cafe", groceries).CreateAsync();

        OverviewModel overview = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-20&history=ThreeMonths");

        OverviewAttentionGroupModel unposted = Assert.Single(overview.Attention,
            group => group.Kind == OverviewAttentionKindModel.UnpostedTransactions);
        Assert.Equal(2, unposted.Count);
        Assert.Equal(-20m, unposted.Amount);
        Assert.Equal(1, unposted.ItemCount);
        Assert.Equal(new DateOnly(2026, 7, 12), unposted.OldestDate);
        OverviewAttentionItemModel account = Assert.Single(unposted.TopItems);
        Assert.Equal("Cash", account.Name);
        Assert.Equal(2, account.Count);
    }

    /// <summary>
    /// Flags expected income once its date has passed, until that much income has been received.
    /// </summary>
    [Fact]
    public async Task IncomeIsFlaggedOnlyAfterItsDatePassesUnreceived()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        await AddExpectedIncomeAsync(test, july, "Employer", 100m, new DateOnly(2026, 7, 15));

        OverviewModel onDate = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-15&history=ThreeMonths");
        OverviewModel afterDate = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-16&history=ThreeMonths");
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 16)).For(100m).From("Employer").To(cash, groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 16));
        OverviewModel received = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-16&history=ThreeMonths");

        Assert.DoesNotContain(onDate.Attention, group => group.Kind == OverviewAttentionKindModel.IncomeNotReceived);
        OverviewAttentionGroupModel notReceived = Assert.Single(afterDate.Attention,
            group => group.Kind == OverviewAttentionKindModel.IncomeNotReceived);
        Assert.Equal(1, notReceived.Count);
        Assert.Equal(100m, notReceived.Amount);
        Assert.Equal(new DateOnly(2026, 7, 15), notReceived.OldestDate);
        Assert.Equal("Employer", Assert.Single(notReceived.TopItems).Name);
        Assert.DoesNotContain(received.Attention, group => group.Kind == OverviewAttentionKindModel.IncomeNotReceived);
    }

    /// <summary>
    /// Flags goals below their minimum at any time, and goals short of their contribution only once all expected
    /// income has been received.
    /// </summary>
    [Fact]
    public async Task GoalsAreFlaggedBelowMinimumAndBehindAfterIncomeArrives()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Funds.Create("Vacation").In(july).WithGoal(100m).CreateAsync();
        _ = await test.Funds.Create("Emergency").In(july).WithGoal(null, 50m).CreateAsync();
        await AddExpectedIncomeAsync(test, july, "Employer", 200m, new DateOnly(2026, 7, 15));

        OverviewModel beforeIncome = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-10&history=ThreeMonths");
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(200m).From("Employer").To(cash, groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 15));
        OverviewModel afterIncome = await test.Api.GetAsync<OverviewModel>("/overview?asOf=2026-07-16&history=ThreeMonths");

        OverviewAttentionGroupModel belowMinimum = Assert.Single(beforeIncome.Attention,
            group => group.Kind == OverviewAttentionKindModel.GoalsBelowMinimum);
        Assert.Equal(50m, belowMinimum.Amount);
        Assert.Equal("Emergency", Assert.Single(belowMinimum.TopItems).Name);
        Assert.DoesNotContain(beforeIncome.Attention, group => group.Kind == OverviewAttentionKindModel.GoalsBehind);
        OverviewAttentionGroupModel behind = Assert.Single(afterIncome.Attention,
            group => group.Kind == OverviewAttentionKindModel.GoalsBehind);
        Assert.Equal(100m, behind.Amount);
        Assert.Equal("Vacation", Assert.Single(behind.TopItems).Name);
    }

    /// <summary>
    /// Adds an expected income source with a single expected payment to an Accounting Period.
    /// </summary>
    private static async Task AddExpectedIncomeAsync(
        FinancialTrackerTestContext test,
        AccountingPeriodHandle period,
        string name,
        decimal amount,
        DateOnly date) =>
        _ = await test.Api.PostAsync<ExpectedIncomeSourceRequestModel, AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{period.Id}/expected-income-sources",
            new ExpectedIncomeSourceRequestModel
            {
                Name = name,
                IncomeLines = [new CreateIncomeLineModel { Description = "Salary", Amount = amount }],
                IncomeDeductions = [],
                UntrackedTransfers = [],
                ExpectedDates = [date],
            });
}
