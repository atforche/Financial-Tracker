using System.Net;
using Models;
using Models.AccountingPeriods;
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
    /// Returns the periods in a range in the requested order with expected income totaled across them.
    /// </summary>
    [Fact]
    public async Task RangeReturnsSortedPeriodsAndExpectedIncomeTotals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        _ = await test.Api.PostAsync<ExpectedIncomeSourceRequestModel, AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{july.Id}/expected-income-sources",
            new ExpectedIncomeSourceRequestModel
            {
                Name = "Employer",
                IncomeLines = [new CreateIncomeLineModel { Description = "Salary", Amount = 100m }],
                IncomeDeductions = [],
                UntrackedTransfers = [new ExpectedUntrackedIncomeTransferRequestModel { Description = "Transfer", Amount = 20m }],
                ExpectedDates = [new DateOnly(2026, 7, 15)],
            });
        _ = await test.Api.PostAsync<ExpectedIncomeSourceRequestModel, AccountingPeriodWithBalanceModel>(
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
            $"/accounting-periods/range?range.start={july.Id}&range.end={august.Id}&sort=Date");

        Assert.Equal([july.Id, august.Id], range.AccountingPeriods.Items.Select(period => period.Id));
        Assert.Equal(200m, range.TotalExpectedIncome.Total);
        Assert.Equal(180m, range.TotalExpectedIncome.Tracked);
        Assert.Equal(20m, range.TotalExpectedIncome.Untracked);
    }

    /// <summary>
    /// Calculates each period's own income and Fund Goal totals when several periods are returned or a range is paged.
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
        AccountingPeriodsInRangeModel secondPage = await test.Api.GetAsync<AccountingPeriodsInRangeModel>(
            $"/accounting-periods/range?range.start={july.Id}&range.end={august.Id}&sort=Date&offset=1&limit=1");

        Assert.Equal([60m, 40m], balances.Items.Select(period => period.ActualIncome.Total));
        Assert.All(balances.Items, period => Assert.Equal(50m, period.PlannedGoalContributions));
        AccountingPeriodWithBalanceModel pagedPeriod = Assert.Single(secondPage.AccountingPeriods.Items);
        Assert.Equal(august.Id, pagedPeriod.Id);
        Assert.Equal(40m, pagedPeriod.ActualIncome.Total);
        Assert.Equal(50m, pagedPeriod.PlannedGoalContributions);
        Assert.Equal(2, secondPage.AccountingPeriods.TotalCount);
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
        using HttpResponseMessage events = await test.Api.GetResponseAsync($"/fund-goals/balance-events/accounting-period-range?{reversed}");

        Assert.Empty(transactions.Transactions.Items);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missing.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, accounts.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, funds.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reversedTransactions.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, events.StatusCode);
    }

    /// <summary>
    /// Includes pending transactions in an accounting period's transaction page.
    /// </summary>
    [Fact]
    public async Task PeriodTransactionsIncludePendingTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();

        AccountingPeriodWithTransactionsModel period = await test.Api.GetAsync<AccountingPeriodWithTransactionsModel>($"/accounting-periods/{july.Id}/transactions");

        Assert.Contains(period.Transactions.Items, item => item.Id == transaction.Id);
    }
}
