using Models;
using Models.Transactions.Types;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers transaction query filtering, sorting, paging, and polymorphic detail contracts.
/// </summary>
public sealed class TransactionQueryContractTests
{
    /// <summary>
    /// Distinguishes fully posted transactions and orders them through the public query contract.
    /// </summary>
    [Fact]
    public async Task TransactionListSortsByFullyPostedAndReturnsTypedDetails()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle savings = await test.Accounts.Onboard("Savings").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        TransactionHandle pending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", await test.Funds.Create("Groceries").In(july).CreateAsync()).CreateAsync();
        TransactionHandle posted = await test.Transactions.Account().In(july).On(new DateOnly(2026, 7, 16)).For(10m).From(cash).To(savings).CreateAsync();
        await test.Transactions.PostAsync(posted, cash, new DateOnly(2026, 7, 16));
        await test.Transactions.PostAsync(posted, savings, new DateOnly(2026, 7, 16));

        CollectionModel<TransactionModel> transactions = await test.Api.GetAsync<CollectionModel<TransactionModel>>(
            "/transactions?sort=FullyPostedDescending&limit=1");
        AccountTransactionModel detail = await test.Api.GetAsync<AccountTransactionModel>($"/transactions/{posted.Id}");

        TransactionModel first = Assert.Single(transactions.Items);
        Assert.Equal(posted.Id, first.Id);
        Assert.True(first.FullyPosted);
        _ = Assert.Single(detail.Destinations);
        Assert.Equal("Cash", detail.Source.Account!.Account.Name);
        Assert.Equal("Savings", Assert.Single(detail.Destinations).Account!.Account.Name);
        Assert.NotEqual(posted.Id, pending.Id);
    }
}