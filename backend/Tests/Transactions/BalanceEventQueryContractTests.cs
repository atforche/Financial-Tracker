using System.Net;
using Models;
using Models.Accounts;
using Models.FundGoals;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers balance-event paging, ordering, and missing-resource behavior.
/// </summary>
public sealed class BalanceEventQueryContractTests
{
    /// <summary>
    /// Applies descending ordering and limits consistently across each balance-event surface.
    /// </summary>
    [Fact]
    public async Task BalanceEventQueriesOrderAndPageAcrossSurfaces()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle earlier = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(10m).From(cash).To("First", groceries).CreateAsync();
        TransactionHandle later = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 20)).For(20m).From(cash).To("Second", groceries).CreateAsync();

        CollectionModel<AccountBalanceEventModel> accounts = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31&sort=DateDescending&limit=1");
        CollectionModel<FundBalanceEventModel> funds = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31&sort=DateDescending&limit=1");
        CollectionModel<FundGoalBalanceEventModel> goals = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            "/fund-goals/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31&sort=DateDescending&limit=1");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync(
            $"/accounts/{Guid.NewGuid()}/balance-events?range.start=2026-07-01&range.end=2026-07-31");

        Assert.Equal(2, accounts.TotalCount);
        Assert.Equal(later.Id, Assert.Single(accounts.Items).TransactionId);
        Assert.Equal(2, funds.TotalCount);
        Assert.Equal(later.Id, Assert.Single(funds.Items).TransactionId);
        Assert.Equal(2, goals.TotalCount);
        Assert.Equal(later.Id, Assert.Single(goals.Items).TransactionId);
        Assert.DoesNotContain(accounts.Items, item => item.TransactionId == earlier.Id);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    /// <summary>
    /// Applies filters and range validation to Account balance events queried across Accounts.
    /// </summary>
    [Fact]
    public async Task AccountBalanceEventRangeQueriesFilterAndValidateAcrossAccounts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountHandle card = await test.Accounts.Onboard("Card").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle cashTransaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 20)).For(20m).From(card).To("Market", groceries).CreateAsync();

        CollectionModel<AccountBalanceEventModel> byDate = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            "/accounts/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31&filter.names=Cash&sort=AmountDescending");
        CollectionModel<AccountBalanceEventModel> byPeriod = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}&filter.names=Cash&sort=Counterparty");
        using HttpResponseMessage reversed = await test.Api.GetResponseAsync(
            $"/accounts/balance-events/accounting-period-range?range.start={august.Id}&range.end={july.Id}");

        AccountBalanceEventModel dateEvent = Assert.Single(byDate.Items);
        Assert.Equal(cashTransaction.Id, dateEvent.TransactionId);
        Assert.Equal(cash.Id, dateEvent.Account.Id);
        Assert.Equal(1, byDate.TotalCount);
        Assert.Equal(cashTransaction.Id, Assert.Single(byPeriod.Items).TransactionId);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reversed.StatusCode);
    }
}