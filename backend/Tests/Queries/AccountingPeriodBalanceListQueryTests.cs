using Models;
using Models.AccountingPeriods;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Queries;

/// <summary>
/// Covers filtering, sorting, and paging for Accounting Period balance snapshots.
/// </summary>
public sealed class AccountingPeriodBalanceListQueryTests
{
    /// <summary>
    /// Returns period balances with filtered totals and balance ordering.
    /// </summary>
    [Fact]
    public async Task AccountingPeriodBalanceListFiltersSortsAndPages()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 15));

        CollectionModel<AccountingPeriodWithBalanceModel> response = await test.Api.GetAsync<CollectionModel<AccountingPeriodWithBalanceModel>>(
            "/accounting-periods/with-balances?filter.months=7&filter.months=8&sort=ClosingBalanceDescending&offset=1&limit=1");

        Assert.Equal(2, response.TotalCount);
        AccountingPeriodWithBalanceModel period = Assert.Single(response.Items);
        Assert.Equal(july.Id, period.Id);
        Assert.Equal(100m, period.OpeningBalance);
        Assert.Equal(90m, period.ClosingBalance);
        Assert.NotEqual(july.Id, august.Id);
    }
}
