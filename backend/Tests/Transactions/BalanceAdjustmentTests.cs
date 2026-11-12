using System.Net;
using Models.Accounts;
using Models.Transactions;
using Models.Transactions.Types;

namespace Tests.Transactions;

/// <summary>
/// Tests for Balance Adjustments, which are always posted on their date and store a signed change to the displayed balance.
/// </summary>
public sealed class BalanceAdjustmentTests
{
    /// <summary>
    /// Creating, updating, and deleting an adjustment keeps it posted and keeps balances synchronized.
    /// </summary>
    [Fact]
    public async Task CreateUpdateAndDeleteKeepAdjustmentPosted()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle brokerage = await test.Accounts.Onboard("Brokerage").WithType(AccountTypeModel.Investment).WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        BalanceAdjustmentBuilder adjustment = test.Transactions.BalanceAdjustment().In(july).On(new DateOnly(2026, 7, 15)).For(50m).Of(brokerage);

        TransactionHandle transaction = await adjustment.CreateAsync();
        await test.AssertBalanceAsync(brokerage, 1050m, 1050m);
        await test.AssertBalanceAsync(july, 1000m, 1050m);
        await test.AssertBalanceAsync(august, 1050m, 1050m);

        await adjustment.For(-30m).On(new DateOnly(2026, 7, 20)).UpdateAsync(transaction);
        await test.AssertBalanceAsync(brokerage, 970m, 970m);
        await test.AssertBalanceAsync(july, 1000m, 970m);
        await test.AssertBalanceAsync(august, 970m, 970m);

        BalanceAdjustmentModel model = await test.Api.GetAsync<BalanceAdjustmentModel>($"/transactions/{transaction.Id}");
        Assert.Equal(-30m, model.Amount);
        Assert.Equal(new DateOnly(2026, 7, 20), model.Date);
        Assert.True(model.FullyPosted);
        Assert.Equal(brokerage.Id, model.Account.Account.Id);

        await test.Transactions.DeleteAsync(transaction);
        await test.AssertBalanceAsync(brokerage, 1000m, 1000m);
        await test.AssertBalanceAsync(july, 1000m, 1000m);
        await test.AssertBalanceAsync(august, 1000m, 1000m);
    }

    /// <summary>
    /// A positive adjustment to a Debt account means more is owed.
    /// </summary>
    [Fact]
    public async Task PositiveDebtAdjustmentIncreasesAmountOwed()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle loan = await test.Accounts.Onboard("Loan").WithType(AccountTypeModel.Debt).WithOpeningBalance(500m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        _ = await test.Transactions.BalanceAdjustment().In(july).On(new DateOnly(2026, 7, 15)).For(25m).Of(loan).CreateAsync();

        await test.AssertBalanceAsync(loan, 525m, 525m);
        await test.AssertBalanceAsync(july, -400m, -425m);
    }

    /// <summary>
    /// Updating an earlier adjustment replays later posted balances without recalculating later adjustments.
    /// </summary>
    [Fact]
    public async Task UpdatingEarlierAdjustmentShiftsLaterBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle retirement = await test.Accounts.Onboard("401k").WithType(AccountTypeModel.Retirement).WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        BalanceAdjustmentBuilder earlierBuilder = test.Transactions.BalanceAdjustment().In(july).On(new DateOnly(2026, 7, 10)).For(10m).Of(retirement);
        TransactionHandle earlier = await earlierBuilder.CreateAsync();
        TransactionHandle later = await test.Transactions.BalanceAdjustment().In(july).On(new DateOnly(2026, 7, 20)).For(5m).Of(retirement).CreateAsync();

        await earlierBuilder.For(20m).UpdateAsync(earlier);

        await test.AssertBalanceAsync(retirement, 125m, 125m);
        BalanceAdjustmentModel laterModel = await test.Api.GetAsync<BalanceAdjustmentModel>($"/transactions/{later.Id}");
        Assert.Equal(5m, laterModel.Amount);
    }

    /// <summary>
    /// Adjustments are limited to untracked balance accounts, must change the balance, and cannot be posted or unposted.
    /// </summary>
    [Fact]
    public async Task MutationsRejectInvalidAdjustments()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle checking = await test.Accounts.Onboard("Checking").WithOpeningBalance(100m).CreateAsync();
        AccountHandle brokerage = await test.Accounts.Onboard("Brokerage").WithType(AccountTypeModel.Investment).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        TransactionHandle transaction = await test.Transactions.BalanceAdjustment().In(july).On(new DateOnly(2026, 7, 15)).For(10m).Of(brokerage).CreateAsync();

        using HttpResponseMessage standardAccount = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.BalanceAdjustment().In(july).On(new DateOnly(2026, 7, 15)).For(10m).Of(checking).CreateRequest());
        using HttpResponseMessage zeroAmount = await test.Api.PostResponseAsync("/transactions",
            test.Transactions.BalanceAdjustment().In(july).On(new DateOnly(2026, 7, 15)).For(0m).Of(brokerage).CreateRequest());
        using HttpResponseMessage post = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/post", new PostTransactionModel
        {
            AccountId = brokerage.Id,
            Date = new DateOnly(2026, 7, 16)
        });
        using HttpResponseMessage unpost = await test.Api.PostResponseAsync<object?>($"/transactions/{transaction.Id}/unpost", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, standardAccount.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, zeroAmount.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, post.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unpost.StatusCode);
        await test.AssertBalanceAsync(brokerage, 10m, 10m);
    }
}
