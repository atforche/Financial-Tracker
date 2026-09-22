using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.AccountGoals;
using Models.FundGoals;
using Models.Funds;
using Models.Transactions.Create;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Pages;

/// <summary>
/// Characterizes the current accounting period workspace, Cash Flow, and trends reads.
/// </summary>
public sealed class AccountingPeriodPageReadTests
{
    /// <summary>
    /// Gives the workspace period options and balance cards for the same periods.
    /// </summary>
    [Fact]
    public async Task WorkspaceReturnsPeriodsAndTheirBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?sort=DateDescending&limit=500&offset=0");
        CollectionModel<AccountingPeriodWithBalanceModel> balances = await test.Api.GetAsync<CollectionModel<AccountingPeriodWithBalanceModel>>(
            "/accounting-periods/with-balances?limit=1&offset=0");

        Assert.Contains(periods.Items, item => item.Id == july.Id);
        Assert.Contains(periods.Items, item => item.Id == august.Id);
        Assert.Equal(2, balances.TotalCount);
        Assert.Equal(100m, Assert.Single(balances.Items).ClosingBalance);
    }

    /// <summary>
    /// Keeps a period's transaction list and account/fund summaries on the same financial facts.
    /// </summary>
    [Fact]
    public async Task CashFlowReturnsMatchingTransactionAndBalanceSummaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 7, 15));

        AccountingPeriodWithBalanceModel period = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{july.Id}");
        AccountingPeriodWithTransactionsModel transactions = await test.Api.GetAsync<AccountingPeriodWithTransactionsModel>(
            $"/accounting-periods/{july.Id}/transactions?limit=1&offset=0");
        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={july.Id}&limit=1");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={july.Id}&limit=1");

        Assert.Equal(80m, period.ClosingBalance);
        Assert.Equal(spending.Id, Assert.Single(transactions.Transactions.Items).Id);
        Assert.Equal(20m, transactions.TotalSpending);
        Assert.Equal(20m, accounts.TotalSpending);
        Assert.Equal(20m, funds.TotalSpending);
    }

    /// <summary>
    /// Returns range totals independent of the paged period rows shown in trends.
    /// </summary>
    [Fact]
    public async Task TrendsKeepsRangeTotalsWhenPeriodRowsArePaged()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(august).On(new DateOnly(2026, 8, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 8, 15));

        AccountingPeriodsInRangeModel periods = await test.Api.GetAsync<AccountingPeriodsInRangeModel>(
            $"/accounting-periods/range?range.start={july.Id}&range.end={august.Id}&limit=1&offset=0");
        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}&limit=1");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}&limit=1");

        Assert.Equal(2, periods.AccountingPeriods.TotalCount);
        _ = Assert.Single(periods.AccountingPeriods.Items);
        Assert.Equal(20m, periods.TotalSpending);
        Assert.Equal(20m, accounts.TotalSpending);
        Assert.Equal(20m, funds.TotalSpending);
    }

    /// <summary>
    /// Keeps the Plan period and both sets of goals and progress in the same period.
    /// </summary>
    [Fact]
    public async Task PlanReturnsPeriodAndMatchingGoalProgresses()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        AccountingPeriodWithBalanceModel period = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{july.Id}");
        CollectionModel<AccountGoalModel> accountGoals = await test.Api.GetAsync<CollectionModel<AccountGoalModel>>(
            $"/account-goals?filter.accountingPeriodIds={july.Id}");
        CollectionModel<FundGoalModel> fundGoals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>(
            $"/fund-goals?filter.accountingPeriodIds={july.Id}");
        IReadOnlyCollection<AccountGoalProgressResultModel> accountProgress = await test.Api.GetAsync<IReadOnlyCollection<AccountGoalProgressResultModel>>(
            $"/account-goals/progress/{july.Id}");
        IReadOnlyCollection<FundGoalProgressResultModel> fundProgress = await test.Api.GetAsync<IReadOnlyCollection<FundGoalProgressResultModel>>(
            $"/fund-goals/progress/{july.Id}");

        Assert.Equal(july.Id, period.Id);
        AccountGoalModel accountGoal = Assert.Single(accountGoals.Items, item => item.Account.Id == cash.Id);
        FundGoalModel fundGoal = Assert.Single(fundGoals.Items, item => item.Fund.Id == groceries.Id);
        Assert.Contains(accountProgress, item => item.AccountGoalId == accountGoal.Id);
        Assert.Contains(fundProgress, item => item.FundGoalId == fundGoal.Id);
    }

    /// <summary>
    /// Supplies the expected income source detail and form from the period read.
    /// </summary>
    [Fact]
    public async Task ExpectedIncomeSourcePagesReturnSavedSourceFromPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        _ = await test.Api.PostAsync<ExpectedIncomeSourceRequestModel, AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{july.Id}/expected-income-sources",
            new ExpectedIncomeSourceRequestModel
            {
                Name = "Employer",
                IncomeLines = [new CreateIncomeLineModel { Description = "Salary", Amount = 100m }],
                IncomeDeductions = [],
                UntrackedTransfers = [],
                ExpectedDates = [new DateOnly(2026, 7, 15)],
            });

        AccountingPeriodWithBalanceModel period = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{july.Id}");

        Assert.Contains(period.ExpectedIncomeSources, item => item.Name == "Employer");
        Assert.Equal(100m, period.ExpectedIncome.Total);
    }
}
