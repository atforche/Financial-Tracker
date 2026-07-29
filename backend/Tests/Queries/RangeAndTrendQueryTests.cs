using Models.Accounts;
using Models.Funds;
using Models.Transactions;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Queries;

/// <summary>
/// Covers range and trend read projections over posted and pending activity.
/// </summary>
public sealed class RangeAndTrendQueryTests
{
    /// <summary>
    /// Returns account, fund, transaction, and trend projections for the same date range.
    /// </summary>
    [Fact]
    public async Task DateRangeEndpointsReturnFinancialFacts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(80m).From(cash).To("Market", groceries).CreateAsync();

        AccountsInDateRangeModel accounts = await test.Api.GetAsync<AccountsInDateRangeModel>("/accounts/date-range?range.start=2026-07-01&range.end=2026-07-31");
        FundsInDateRangeModel funds = await test.Api.GetAsync<FundsInDateRangeModel>("/funds/date-range?range.start=2026-07-01&range.end=2026-07-31");
        TransactionsInDateRangeModel transactions = await test.Api.GetAsync<TransactionsInDateRangeModel>("/transactions/date-range?range.start=2026-07-01&range.end=2026-07-31");
        TransactionTrendsModel trends = await test.Api.GetAsync<TransactionTrendsModel>("/transactions/trends/date-range?range.start=2026-07-01&range.end=2026-07-31");

        Assert.Contains(accounts.Accounts.Items, account => account.Id == cash.Id);
        Assert.Contains(funds.Funds.Items, fund => fund.Id == groceries.Id);
        Assert.Equal(1, transactions.Transactions.TotalCount);
        Assert.Contains(trends.Dates, date => date.Date == new DateOnly(2026, 7, 15));
    }

    /// <summary>
    /// Returns a range of contiguous accounting periods and rejects a missing endpoint.
    /// </summary>
    [Fact]
    public async Task AccountingPeriodRangeEndpointsRequireContiguousPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        TransactionsInAccountingPeriodRangeModel transactions = await test.Api.GetAsync<TransactionsInAccountingPeriodRangeModel>(
            $"/transactions/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        TransactionTrendsModel trends = await test.Api.GetAsync<TransactionTrendsModel>(
            $"/transactions/trends/accounting-period-range?range.start={july.Id}&range.end={august.Id}");

        Assert.Empty(transactions.Transactions.Items);
        Assert.Empty(trends.AccountingPeriods);
    }

    /// <summary>
    /// Includes transactions on both date-range boundaries and excludes adjacent activity.
    /// </summary>
    [Fact]
    public async Task DateRangeEndpointsUseInclusiveBoundaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 1)).For(10m).From(cash).To("Start", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 31)).For(15m).From(cash).To("End", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(august).On(new DateOnly(2026, 8, 1)).For(20m).From(cash).To("Outside", groceries).CreateAsync();

        TransactionsInDateRangeModel transactions = await test.Api.GetAsync<TransactionsInDateRangeModel>(
            "/transactions/date-range?range.start=2026-07-01&range.end=2026-07-31");

        Assert.Equal(2, transactions.Transactions.TotalCount);
        Assert.Contains(transactions.Transactions.Items, item => item.Description == "Start");
        Assert.Contains(transactions.Transactions.Items, item => item.Description == "End");
        Assert.DoesNotContain(transactions.Transactions.Items, item => item.Description == "Outside");
    }

    /// <summary>
    /// Aggregates transaction trends by type, date, and accounting period after applying filters.
    /// </summary>
    [Fact]
    public async Task TransactionTrendsAggregateFilteredFinancialFacts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(80m).From("Employer").To(cash, groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(30m).From(cash).To("Market", groceries).CreateAsync();

        TransactionTrendsModel periodTrends = await test.Api.GetAsync<TransactionTrendsModel>(
            $"/transactions/trends/accounting-period-range?range.start={july.Id}&range.end={july.Id}&filter.types=Income");
        TransactionTrendsModel dateTrends = await test.Api.GetAsync<TransactionTrendsModel>(
            "/transactions/trends/date-range?range.start=2026-07-01&range.end=2026-07-31&filter.types=Income");

        TransactionSummaryByTypeModel income = Assert.Single(periodTrends.TransactionTypes);
        TransactionSummaryByDateModel date = Assert.Single(dateTrends.Dates);
        TransactionSummaryByPeriodModel period = Assert.Single(periodTrends.AccountingPeriods);
        Assert.Equal(TransactionTypeModel.Income, income.TransactionType);
        Assert.Equal(1, income.TotalCount);
        Assert.Equal(80m, income.TotalAmount);
        Assert.Equal(new DateOnly(2026, 7, 10), date.Date);
        Assert.Equal(80m, date.TotalAmount);
        Assert.Equal(july.Id, period.AccountingPeriod.Id);
        Assert.Equal(80m, period.TotalAmount);
        Assert.Contains("Cash", periodTrends.AvailableAccountNames);
        Assert.Contains("Groceries", periodTrends.AvailableFundNames);
    }
}