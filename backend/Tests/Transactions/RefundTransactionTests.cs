using Models.Accounts;

namespace Tests.Transactions;

/// <summary>
/// Covers how refunds reverse spending effects on account and fund balances.
/// </summary>
public sealed class RefundTransactionTests
{
    /// <summary>
    /// A refund increases account and fund balances while reversing spending.
    /// </summary>
    [Fact]
    public async Task RefundProjectsAndPostsReverseSpendingEffects()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle refund = await test.Transactions.Refund().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From("Market").To(cash, groceries).CreateAsync();
        await test.AssertBalanceAsync(cash, 100m, 125m);
        await test.AssertBalanceAsync(groceries, 0m, 25m);

        await test.Transactions.PostAsync(refund, cash, new DateOnly(2026, 7, 16));
        await test.AssertBalanceAsync(cash, 125m, 125m);
        await test.AssertBalanceAsync(groceries, 25m, 25m);
    }

    /// <summary>
    /// An untracked refund source is independently debited when posted.
    /// </summary>
    [Fact]
    public async Task RefundFromUntrackedAccountDebitsSourceWhenPosted()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle source = await test.Accounts.Onboard("Merchant Credit").WithType(AccountTypeModel.Investment).WithOpeningBalance(50m).CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle refund = await test.Transactions.Refund().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From(source).To(cash, groceries).CreateAsync();
        await test.AssertBalanceAsync(source, 50m, 25m);

        await test.Transactions.PostAsync(refund, source, new DateOnly(2026, 7, 16));
        await test.AssertBalanceAsync(source, 25m, 25m);
        await test.AssertBalanceAsync(cash, 100m, 125m);

        await test.Transactions.PostAsync(refund, cash, new DateOnly(2026, 7, 17));
        await test.AssertBalanceAsync(cash, 125m, 125m);
    }

    /// <summary>
    /// Multiple sources independently debit while their combined amount credits one destination.
    /// </summary>
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
        await test.AssertBalanceAsync(merchantCredit, 40m, 30m);
        await test.AssertBalanceAsync(cash, 100m, 125m);
        await test.AssertBalanceAsync(groceries, 0m, 25m);

        await test.Transactions.PostAsync(refund, merchantCredit, new DateOnly(2026, 7, 16));
        await test.AssertBalanceAsync(merchantCredit, 30m, 30m);
        await test.AssertBalanceAsync(cash, 100m, 125m);

        await test.Transactions.PostAsync(refund, cash, new DateOnly(2026, 7, 17));
        await test.AssertBalanceAsync(cash, 125m, 125m);
        await test.AssertBalanceAsync(groceries, 25m, 25m);
    }
}
