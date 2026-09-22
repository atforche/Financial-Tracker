using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.FundGoals;
using Models.Funds;
using Models.Locations;
using Models.Transactions.Types;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Pages;

/// <summary>
/// Characterizes the transaction workspace and transaction detail/form reads.
/// </summary>
public sealed class TransactionPageReadTests
{
    /// <summary>
    /// Supplies filter choices and a filtered transaction row from the same seeded data.
    /// </summary>
    [Fact]
    public async Task WorkspaceReturnsReferenceChoicesAndFilteredTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?limit=500");
        CollectionModel<AccountWithBalanceModel> accounts = await test.Api.GetAsync<CollectionModel<AccountWithBalanceModel>>(
            "/accounts/with-balances");
        CollectionModel<FundWithBalanceModel> funds = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>(
            "/funds/with-balances");
        CollectionModel<LocationModel> locations = await test.Api.GetAsync<CollectionModel<LocationModel>>(
            "/locations?limit=500");
        CollectionModel<TransactionModel> transactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            $"/transactions?filter.accountingPeriodIds={july.Id}&limit=5");

        Assert.Contains(periods.Items, item => item.Id == july.Id);
        Assert.Contains(accounts.Items, item => item.Id == cash.Id);
        Assert.Contains(funds.Items, item => item.Id == groceries.Id);
        Assert.Contains(locations.Items, item => item.Name == "Market");
        Assert.Equal(spending.Id, Assert.Single(transactions.Items).Id);
    }

    /// <summary>
    /// Supplies the create form with one open period and the matching fund goal progress.
    /// </summary>
    [Fact]
    public async Task CreateFormReturnsOpenPeriodAndGoalReferenceData()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?limit=500");
        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>(
            $"/fund-goals?filter.accountingPeriodIds={july.Id}&limit=500");
        IReadOnlyCollection<FundGoalProgressResultModel> progresses = await test.Api.GetAsync<IReadOnlyCollection<FundGoalProgressResultModel>>(
            $"/fund-goals/progress/{july.Id}");

        Assert.Contains(periods.Items, item => item.Id == july.Id && item.IsOpen);
        FundGoalModel goal = Assert.Single(goals.Items, item => item.Fund.Id == groceries.Id);
        Assert.Contains(progresses, item => item.FundGoalId == goal.Id);
    }

    /// <summary>
    /// Returns a transaction and the period and fund goal reference data used by its detail page.
    /// </summary>
    [Fact]
    public async Task DetailReturnsTransactionAndMatchingPeriodReferences()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();

        SpendingTransactionModel transaction = await test.Api.GetAsync<SpendingTransactionModel>(
            $"/transactions/{spending.Id}");
        AccountingPeriodWithBalanceModel period = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{transaction.AccountingPeriodId}");
        CollectionModel<FundGoalModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalModel>>(
            $"/fund-goals?filter.accountingPeriodIds={july.Id}&limit=500");
        IReadOnlyCollection<FundGoalProgressResultModel> progresses = await test.Api.GetAsync<IReadOnlyCollection<FundGoalProgressResultModel>>(
            $"/fund-goals/progress/{july.Id}");

        Assert.Equal(20m, transaction.Amount);
        Assert.Equal(period.Id, transaction.AccountingPeriodId);
        FundGoalModel goal = Assert.Single(goals.Items, item => item.Fund.Id == groceries.Id);
        Assert.Contains(progresses, item => item.FundGoalId == goal.Id);
    }

    /// <summary>
    /// Lets the edit form resolve its transaction period from the full period choices.
    /// </summary>
    [Fact]
    public async Task EditFormFindsTransactionAccountingPeriodInReferenceData()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();

        SpendingTransactionModel transaction = await test.Api.GetAsync<SpendingTransactionModel>(
            $"/transactions/{spending.Id}");
        CollectionModel<AccountingPeriodModel> periods = await test.Api.GetAsync<CollectionModel<AccountingPeriodModel>>(
            "/accounting-periods?limit=500");

        Assert.Equal(20m, transaction.Amount);
        Assert.Contains(periods.Items, item => item.Id == transaction.AccountingPeriodId);
    }
}
