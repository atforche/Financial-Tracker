using System.Net;
using Models.Transactions;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers transaction state-transition guards that protect persisted balance histories.
/// </summary>
public sealed class TransactionLifecycleBoundaryTests
{
    /// <summary>
    /// Rejects duplicate posts, dates before the transaction, and dates outside the accounting-period posting window.
    /// </summary>
    [Fact]
    public async Task PostAsyncEnforcesTransactionDateAndPostingWindow()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();

        using HttpResponseMessage beforeTransaction = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/post", new PostTransactionModel
        {
            AccountId = cash.Id,
            Date = new DateOnly(2026, 7, 14)
        });
        using HttpResponseMessage outsideWindow = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/post", new PostTransactionModel
        {
            AccountId = cash.Id,
            Date = new DateOnly(2026, 9, 1)
        });

        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        using HttpResponseMessage duplicate = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/post", new PostTransactionModel
        {
            AccountId = cash.Id,
            Date = new DateOnly(2026, 7, 16)
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, beforeTransaction.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, outsideWindow.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);
    }

    /// <summary>
    /// Rejects unposting a pending transaction and posting or unposting a Fund transfer.
    /// </summary>
    [Fact]
    public async Task PostAndUnpostAsyncRejectInvalidTransactionStates()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle dining = await test.Funds.Create("Dining").In(july).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        TransactionHandle transfer = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(groceries).To(dining).CreateAsync();

        using HttpResponseMessage pendingUnpost = await test.Api.PostResponseAsync($"/transactions/{spending.Id}/unpost", new { });
        using HttpResponseMessage fundPost = await test.Api.PostResponseAsync($"/transactions/{transfer.Id}/post", new PostTransactionModel
        {
            AccountId = cash.Id,
            Date = new DateOnly(2026, 7, 15)
        });
        using HttpResponseMessage fundUnpost = await test.Api.PostResponseAsync($"/transactions/{transfer.Id}/unpost", new { });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, pendingUnpost.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fundPost.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fundUnpost.StatusCode);
    }

    /// <summary>
    /// Rejects unposting and deletion once a transaction's accounting period is closed.
    /// </summary>
    [Fact]
    public async Task ClosedPeriodRejectsTransactionMutations()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        await test.Api.PostAsync($"/accounting-periods/{july.Id}/close", new { });

        using HttpResponseMessage unpost = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/unpost", new { });
        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/transactions/{transaction.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, unpost.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, delete.StatusCode);
    }
}
