using Models.Accounts;
using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Queries;

/// <summary>
/// Covers financial values returned by date and accounting-period range projections.
/// </summary>
public sealed class FinancialRangeProjectionTests
{
    /// <summary>
    /// Returns posted financial totals and balance boundaries across contiguous periods.
    /// </summary>
    [Fact]
    public async Task RangesReturnPostedTotalsAndBalanceBoundaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(august).On(new DateOnly(2026, 8, 15)).For(15m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 15));
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 8, 15));

        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}");

        AccountWithBalanceRangeModel account = Assert.Single(accounts.Accounts.Items, item => item.Id == cash.Id);
        FundWithBalanceRangeModel fund = Assert.Single(funds.Funds.Items, item => item.Id == groceries.Id);
        Assert.Equal(100m, account.StartingBalance);
        Assert.Equal(125m, account.EndingBalance);
        Assert.Equal(40m, accounts.TotalIncome.Total);
        Assert.Equal(15m, accounts.TotalSpending);
        Assert.Equal(0m, fund.StartingBalance);
        Assert.Equal(25m, fund.EndingBalance);
        Assert.Equal(2, accounts.AccountingPeriods.Count);
        Assert.Equal(2, funds.AccountingPeriods.Count);
    }

    /// <summary>
    /// Recognizes external pending income immediately but waits for internal income to post.
    /// </summary>
    [Fact]
    public async Task DateRangeTotalsRespectIncomeSourceAndPostingState()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountModel investment = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Investment",
            Type = AccountTypeModel.Investment,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1)
        });
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        TransactionHandle external = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From("Employer").To(cash, income).CreateAsync();
        CreateTransactionResultModel internalResult = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 16),
            Description = "Transfer",
            Amount = 30m,
            Source = new CreateIncomeTransactionSourceModel
            {
                AccountId = investment.Id,
                IncomeLines = [new CreateIncomeLineModel { Description = "Transfer", Amount = 30m }],
                IncomeDeductions = []
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = savings.Id,
                Amount = 30m,
                FundAssignments = [new CreateFundAmountModel { FundId = income.Id, Amount = 30m }]
            }]
        });

        AccountsInDateRangeModel pending = await test.Api.GetAsync<AccountsInDateRangeModel>("/accounts/date-range?range.start=2026-07-01&range.end=2026-07-31");
        await test.Transactions.PostAsync(new TransactionHandle(internalResult.Id), savings, new DateOnly(2026, 7, 16));
        AccountsInDateRangeModel posted = await test.Api.GetAsync<AccountsInDateRangeModel>("/accounts/date-range?range.start=2026-07-01&range.end=2026-07-31");

        Assert.Equal(20m, pending.TotalIncome.Total);
        Assert.Equal(20m, pending.TotalIncome.Tracked);
        Assert.Equal(50m, posted.TotalIncome.Total);
        Assert.Equal(50m, posted.TotalIncome.Tracked);
        Assert.NotEqual(external.Id, internalResult.Id);
    }
}
