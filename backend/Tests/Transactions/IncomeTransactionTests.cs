using Models.Accounts;
using Models.Funds;
using Models.Locations;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;
using Models.Transactions.Update;

namespace Tests.Transactions;

/// <summary>
/// Covers how income affects account, fund, and accounting-period balances, including later periods.
/// </summary>
public sealed class IncomeTransactionTests
{
    /// <summary>
    /// Updating, posting, unposting, and deleting income keeps each balance surface synchronized.
    /// </summary>
    [Fact]
    public async Task UpdatePostUnpostAndDeleteAsyncKeepBalanceSurfacesSynchronized()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        IncomeTransactionBuilder builder = test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(80m).From("Employer").To(cash, income);
        TransactionHandle transaction = await builder.CreateAsync();

        await builder.For(125m).UpdateAsync(transaction);
        await test.AssertBalanceAsync(cash, 1000m, 1125m);
        await test.AssertBalanceAsync(income, 0m, 125m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 20));
        await test.AssertBalanceAsync(cash, 1125m, 1125m);
        await test.AssertBalanceAsync(income, 125m, 125m);
        await test.AssertBalanceAsync(july, 1000m, 1125m);

        await test.Transactions.UnpostAsync(transaction);
        await test.AssertBalanceAsync(cash, 1000m, 1125m);
        await test.AssertBalanceAsync(income, 0m, 125m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);

        await test.Transactions.DeleteAsync(transaction);
        await test.AssertBalanceAsync(cash, 1000m, 1000m);
        await test.AssertBalanceAsync(income, 0m, 0m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);
    }

    /// <summary>
    /// Posts each income destination independently while keeping the others, and their funds, pending.
    /// </summary>
    [Fact]
    public async Task PostAsyncKeepsUnpostedDestinationsAndTheirFundsPending()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle first = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle second = await test.Accounts.Onboard("Second").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle firstFund = await test.Funds.Create("First fund").In(july).CreateAsync();
        FundHandle secondFund = await test.Funds.Create("Second fund").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(100m)
            .From("Employer").To(first, firstFund, 40m).To(second, secondFund, 60m).CreateAsync();

        await test.Transactions.PostAsync(transaction, first, new DateOnly(2026, 7, 16));
        await test.AssertBalanceAsync(first, 40m, 40m);
        await test.AssertBalanceAsync(second, 0m, 60m);
        await test.AssertBalanceAsync(firstFund, 40m, 40m);
        await test.AssertBalanceAsync(secondFund, 0m, 60m);

        await test.Transactions.PostAsync(transaction, second, new DateOnly(2026, 7, 17));
        await test.AssertBalanceAsync(second, 60m, 60m);
        await test.AssertBalanceAsync(secondFund, 60m, 60m);

        await test.Transactions.UnpostAsync(transaction);
        await test.AssertBalanceAsync(first, 0m, 40m);
        await test.AssertBalanceAsync(second, 0m, 60m);
        await test.AssertBalanceAsync(firstFund, 0m, 40m);
        await test.AssertBalanceAsync(secondFund, 0m, 60m);

        await test.Transactions.DeleteAsync(transaction);
        await test.AssertBalanceAsync(first, 0m, 0m);
        await test.AssertBalanceAsync(second, 0m, 0m);
        await test.AssertBalanceAsync(firstFund, 0m, 0m);
        await test.AssertBalanceAsync(secondFund, 0m, 0m);
    }

    /// <summary>
    /// Posting, unposting, and reposting earlier income updates the opening balances of a later period.
    /// </summary>
    [Fact]
    public async Task EarlierIncomeLifecycleUpdatesLaterPeriodOpeningBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        IncomeTransactionBuilder builder = test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(40m).From("Employer").To(cash, income);
        TransactionHandle transaction = await builder.CreateAsync();

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 15));
        await AssertAugustOpeningAsync(test, july, august, 140m, 40m);

        await test.Transactions.UnpostAsync(transaction);
        await AssertAugustOpeningAsync(test, july, august, 100m, 0m);

        await builder.For(65m).UpdateAsync(transaction);
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 15));
        await AssertAugustOpeningAsync(test, july, august, 165m, 65m);
    }

    /// <summary>
    /// Propagates each destination's posting into later periods, including postings made after the period closes.
    /// </summary>
    [Fact]
    public async Task PartialPostingPropagatesOnlyPostedDestinationsToLaterPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle first = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle second = await test.Accounts.Onboard("Second").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle firstFund = await test.Funds.Create("First fund").In(july).CreateAsync();
        FundHandle secondFund = await test.Funds.Create("Second fund").In(july).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(100m)
            .From("Employer").To(first, firstFund, 40m).To(second, secondFund, 60m).CreateAsync();

        await test.Transactions.PostAsync(transaction, first, new DateOnly(2026, 7, 16));
        await AssertAugustOpeningAsync(test, july, august, 40m, 40m);

        await test.Periods.CloseAsync(july);
        await test.Transactions.PostAsync(transaction, second, new DateOnly(2026, 7, 17));
        await AssertAugustOpeningAsync(test, july, august, 100m, 100m);

        await test.Periods.ReopenAsync(july);
        await test.Transactions.UnpostAsync(transaction);
        await AssertAugustOpeningAsync(test, july, august, 0m, 0m);
    }

    /// <summary>
    /// Moving and reposting earlier income rebuilds the later posted account, fund, and period histories.
    /// </summary>
    [Fact]
    public async Task RepostEarlierIncomeReplaysLaterPostedBalanceHistories()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        IncomeTransactionBuilder earlierBuilder = test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 20)).For(20m).From("Employer").To(cash, income);
        TransactionHandle earlier = await earlierBuilder.CreateAsync();
        TransactionHandle later = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 21)).For(30m).From("Employer").To(cash, income).CreateAsync();
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 20));
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 21));

        await test.Transactions.UnpostAsync(earlier);
        await earlierBuilder.On(new DateOnly(2026, 7, 15)).For(25m).UpdateAsync(earlier);
        await test.Transactions.PostAsync(earlier, cash, new DateOnly(2026, 7, 15));

        await test.AssertBalanceAsync(cash, 155m, 155m);
        await test.AssertBalanceAsync(income, 55m, 55m);
        await test.AssertBalanceAsync(july, 100m, 155m);
    }

    /// <summary>
    /// An untracked income source account posts independently from its tracked destination.
    /// </summary>
    [Fact]
    public async Task IncomeFromUntrackedAccountPostsSourceAndDestinationIndependently()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(10m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountHandle card = await test.Accounts.Create("Card", july).WithType(AccountTypeModel.Debt).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 15)).For(40m).From(card).To(cash, income).CreateAsync();

        await test.Transactions.PostAsync(transaction, card, new DateOnly(2026, 7, 16));
        await test.AssertBalanceAsync(card, 40m, 40m);
        await test.AssertBalanceAsync(cash, 10m, 50m);
        await test.AssertBalanceAsync(income, 0m, 40m);

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 17));
        await test.AssertBalanceAsync(card, 40m, 40m);
        await test.AssertBalanceAsync(cash, 50m, 50m);
        await test.AssertBalanceAsync(income, 40m, 40m);
    }

    /// <summary>
    /// Changing income's destination removes every pending effect from the old account and fund.
    /// </summary>
    [Fact]
    public async Task UpdateAsyncMovesEffectsToNewAccountAndFund()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle first = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle second = await test.Accounts.Onboard("Second").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle oldFund = await test.Funds.Create("Old").In(july).CreateAsync();
        FundHandle newFund = await test.Funds.Create("New").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(25m).From("Employer").To(first, oldFund).CreateAsync();

        await test.Transactions.Income().On(new DateOnly(2026, 7, 10)).For(25m).From("Employer").To(second, newFund).UpdateAsync(income);

        await test.AssertBalanceAsync(first, 0m, 0m);
        await test.AssertBalanceAsync(second, 0m, 25m);
        await test.AssertBalanceAsync(oldFund, 0m, 0m);
        await test.AssertBalanceAsync(newFund, 0m, 25m);
    }

    /// <summary>
    /// Persists deductions through create, update, read, and posting operations.
    /// </summary>
    [Fact]
    public async Task IncomeDeductionsReconcileAndProjectAcrossReadAndBalanceSurfaces()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateIncomeTransactionModel
        {
            ExpectedIncomeSourceId = null,
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Paycheck",
            Amount = 100m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = new LocationInputModel { NewLocationName = "Employer" },
                IncomeLines = [
                    new CreateIncomeLineModel { Description = "Salary", Amount = 120m },
                    new CreateIncomeLineModel { Description = "Bonus", Amount = 30m }
                ],
                IncomeDeductions = [new CreateIncomeDeductionModel { Description = "Tax", Amount = 50m }]
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 100m,
                FundAssignments = [new CreateFundAmountModel { FundId = income.Id, Amount = 100m }]
            }]
        });
        TransactionHandle transaction = new(created.Id);

        UpdateTransactionModel update = new UpdateIncomeTransactionModel
        {
            ExpectedIncomeSourceId = null,
            Date = new DateOnly(2026, 7, 16),
            Description = "Updated paycheck",
            Amount = 150m,
            Source = new CreateIncomeTransactionSourceModel
            {
                Location = new LocationInputModel { NewLocationName = "Employer" },
                IncomeLines = [new CreateIncomeLineModel { Description = "Salary", Amount = 200m }],
                IncomeDeductions = [
                    new CreateIncomeDeductionModel { Description = "Tax", Amount = 40m },
                    new CreateIncomeDeductionModel { Description = "Benefits", Amount = 10m }
                ]
            },
            Destinations = [new CreateIncomeTransactionDestinationModel
            {
                AccountId = cash.Id,
                Amount = 150m,
                FundAssignments = [new CreateFundAmountModel { FundId = income.Id, Amount = 150m }]
            }]
        };
        await test.Api.PostAsync($"/transactions/{transaction.Id}", update);

        IncomeTransactionModel detail = await test.Api.GetAsync<IncomeTransactionModel>($"/transactions/{transaction.Id}");
        Assert.Equal(150m, detail.Amount);
        Assert.Equal(150m, detail.TrackedAmount);
        Assert.Equal(["Tax", "Benefits"], detail.Source.IncomeDeductions.Select(deduction => deduction.Description));
        Assert.Equal(50m, detail.Source.IncomeDeductions.Sum(deduction => deduction.Amount));

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        await test.AssertBalanceAsync(cash, 250m, 250m);
        await test.AssertBalanceAsync(income, 150m, 150m);
    }

    private static async Task AssertAugustOpeningAsync(
        FinancialTrackerTestContext test,
        AccountingPeriodHandle july,
        AccountingPeriodHandle august,
        decimal expectedAccountBalance,
        decimal expectedFundBalance)
    {
        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}");

        Assert.Equal(expectedAccountBalance, Assert.Single(accounts.AccountingPeriods, item => item.AccountingPeriod.Id == august.Id).OpeningBalance.TotalBalance);
        Assert.Equal(expectedFundBalance, Assert.Single(funds.AccountingPeriods, item => item.AccountingPeriod.Id == august.Id).OpeningBalance.TotalAssignedBalance);
    }
}
