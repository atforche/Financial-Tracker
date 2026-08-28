using Models.Accounts;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>Coverage for refund balance effects.</summary>
public sealed class RefundTransactionLifecycleTests
{
    /// <summary>A refund increases account and fund balances while reversing spending.</summary>
    [Fact]
    public async Task RefundProjectsAndPostsReverseSpendingEffects()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle refund = await test.Transactions.Refund().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From("Market").To(cash, groceries).CreateAsync();

        AccountBalanceSnapshot pendingAccount = await test.AccountQueries.GetBalanceAsync(cash);
        FundBalanceSnapshot pendingFund = await test.FundQueries.GetBalanceAsync(groceries);
        FundGoalAvailabilitySnapshot pendingGoal = await test.FundGoalQueries.GetAvailabilityAsync(groceries.Goal);
        Assert.Equal(125m, pendingAccount.IncludingPending);
        Assert.Equal(25m, pendingFund.IncludingPending);
        Assert.Equal(25m, pendingGoal.IncludingPending);

        await test.Transactions.PostAsync(refund, cash, new DateOnly(2026, 7, 16));
        Assert.Equal(125m, (await test.AccountQueries.GetBalanceAsync(cash)).Posted);
        Assert.Equal(25m, (await test.FundQueries.GetBalanceAsync(groceries)).Posted);
        Assert.Equal(25m, (await test.FundGoalQueries.GetAvailabilityAsync(groceries.Goal)).Posted);
    }

    /// <summary>An untracked refund source is independently debited when posted.</summary>
    [Fact]
    public async Task RefundFromUntrackedAccountDebitsSourceWhenPosted()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle source = await test.Accounts.Onboard("Merchant Credit").WithType(AccountTypeModel.Investment).WithOpeningBalance(50m).CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle refund = await test.Transactions.Refund().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From(source).To(cash, groceries).CreateAsync();

        Assert.Equal(25m, (await test.AccountQueries.GetBalanceAsync(source)).IncludingPending);
        await test.Transactions.PostAsync(refund, source, new DateOnly(2026, 7, 16));
        Assert.Equal(25m, (await test.AccountQueries.GetBalanceAsync(source)).Posted);
        Assert.Equal(100m, (await test.AccountQueries.GetBalanceAsync(cash)).Posted);

        await test.Transactions.PostAsync(refund, cash, new DateOnly(2026, 7, 17));
        Assert.Equal(125m, (await test.AccountQueries.GetBalanceAsync(cash)).Posted);
    }

    /// <summary>Multiple sources independently debit while their combined amount credits one destination.</summary>
    [Fact]
    public async Task RefundSupportsMultipleSourcesAndOneDestination()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle merchantCredit = await test.Accounts.Onboard("Merchant Credit").WithType(AccountTypeModel.Investment).WithOpeningBalance(40m).CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle refund = await test.Transactions.Refund().In(july).On(new DateOnly(2026, 7, 15)).For(25m)
            .From(merchantCredit, 10m).From("Market", 15m).To(cash, groceries).CreateAsync();

        Assert.Equal(30m, (await test.AccountQueries.GetBalanceAsync(merchantCredit)).IncludingPending);
        Assert.Equal(125m, (await test.AccountQueries.GetBalanceAsync(cash)).IncludingPending);
        Assert.Equal(25m, (await test.FundQueries.GetBalanceAsync(groceries)).IncludingPending);

        await test.Transactions.PostAsync(refund, merchantCredit, new DateOnly(2026, 7, 16));
        Assert.Equal(30m, (await test.AccountQueries.GetBalanceAsync(merchantCredit)).Posted);
        Assert.Equal(100m, (await test.AccountQueries.GetBalanceAsync(cash)).Posted);
        await test.Transactions.PostAsync(refund, cash, new DateOnly(2026, 7, 17));
        Assert.Equal(125m, (await test.AccountQueries.GetBalanceAsync(cash)).Posted);
        Assert.Equal(25m, (await test.FundQueries.GetBalanceAsync(groceries)).Posted);
    }

    /// <summary>Multiple account sources are independently posted and debited.</summary>
    [Fact]
    public async Task RefundSupportsMultipleAccountSources()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle firstSource = await test.Accounts.Onboard("First Merchant Credit").WithType(AccountTypeModel.Investment).WithOpeningBalance(20m).CreateAsync();
        AccountHandle secondSource = await test.Accounts.Onboard("Second Merchant Credit").WithType(AccountTypeModel.Investment).WithOpeningBalance(20m).CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle refund = await test.Transactions.Refund().In(july).On(new DateOnly(2026, 7, 15)).For(25m)
            .From(firstSource, 10m).From(secondSource, 15m).To(cash, groceries).CreateAsync();

        await test.Transactions.PostAsync(refund, firstSource, new DateOnly(2026, 7, 16));
        await test.Transactions.PostAsync(refund, secondSource, new DateOnly(2026, 7, 16));
        await test.Transactions.PostAsync(refund, cash, new DateOnly(2026, 7, 17));

        Assert.Equal(10m, (await test.AccountQueries.GetBalanceAsync(firstSource)).Posted);
        Assert.Equal(5m, (await test.AccountQueries.GetBalanceAsync(secondSource)).Posted);
        Assert.Equal(125m, (await test.AccountQueries.GetBalanceAsync(cash)).Posted);
    }
}
