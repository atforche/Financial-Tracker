using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Queries;

/// <summary>
/// Covers current-balance and period snapshot endpoint contracts.
/// </summary>
public sealed class SnapshotEndpointTests
{
    /// <summary>
    /// Returns pending-aware account and fund snapshots and the period transaction page.
    /// </summary>
    [Fact]
    public async Task SnapshotEndpointsReturnCurrentBalancesAndPeriodTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();

        CollectionModel<AccountWithBalanceModel> accounts = await test.Api.GetAsync<CollectionModel<AccountWithBalanceModel>>("/accounts/with-balances");
        CollectionModel<FundWithBalanceModel> funds = await test.Api.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        AccountingPeriodWithTransactionsModel period = await test.Api.GetAsync<AccountingPeriodWithTransactionsModel>($"/accounting-periods/{july.Id}/transactions");

        AccountWithBalanceModel account = Assert.Single(accounts.Items, item => item.Id == cash.Id);
        FundWithBalanceModel fund = Assert.Single(funds.Items, item => item.Id == groceries.Id);
        Assert.Equal(100m, account.CurrentBalance.PostedBalance);
        Assert.Equal(80m, account.CurrentBalance.BalanceIncludingPending);
        Assert.Equal(0m, fund.CurrentBalance.PostedBalance);
        Assert.Equal(-20m, fund.CurrentBalance.BalanceIncludingPending);
        Assert.Contains(period.Transactions.Items, item => item.Id == transaction.Id);
    }
}