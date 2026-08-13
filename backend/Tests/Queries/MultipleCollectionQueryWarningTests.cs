using Models;
using Models.Accounts;
using Models.Funds;
using Models.Transactions.Types;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Queries;

/// <summary>
/// Covers queries that materialize multiple related collections under EF Core's warning-as-error test configuration.
/// </summary>
public sealed class MultipleCollectionQueryWarningTests
{
    /// <summary>
    /// Returns transaction, balance-event, and period-range results without compiling a multiple-collection single query.
    /// </summary>
    [Fact]
    public async Task MultiCollectionQueriesUseSplitQueries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").WithOpeningBalance(50m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle travel = await test.Funds.Create("Travel").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(savings).To("Airline", travel).CreateAsync();

        CollectionModel<TransactionModel> transactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>("/transactions?limit=10");
        IncomeTransactionModel incomeDetails = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{income.Id}");
        CollectionModel<AccountBalanceEventModel> accountEvents = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31");
        CollectionModel<FundBalanceEventModel> fundEvents = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31");
        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={july.Id}");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={july.Id}");

        Assert.Contains(transactions.Items, item => item.Id == income.Id);
        Assert.Contains(transactions.Items, item => item.Id == spending.Id);
        _ = Assert.Single(incomeDetails.Source.IncomeLines);
        Assert.Contains(accountEvents.Items, item => item.TransactionId == income.Id);
        Assert.Contains(fundEvents.Items, item => item.TransactionId == spending.Id);
        Assert.Contains(accounts.Accounts.Items, item => item.Id == cash.Id);
        Assert.Contains(accounts.Accounts.Items, item => item.Id == savings.Id);
        Assert.Contains(funds.Funds.Items, item => item.Id == groceries.Id);
        Assert.Contains(funds.Funds.Items, item => item.Id == travel.Id);
    }
}