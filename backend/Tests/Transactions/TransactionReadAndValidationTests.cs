using System.Net;
using Models;
using Models.Accounts;
using Models.FundGoals;
using Models.Funds;
using Models.Transactions.Create;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers transaction validation and the balance-event read projections it produces.
/// </summary>
public sealed class TransactionReadAndValidationTests
{
    /// <summary>
    /// Rejects transactions with an account reference that does not exist.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRejectsUnknownAccountSource()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();

        using HttpResponseMessage response = await test.Api.PostResponseAsync<CreateTransactionModel>("/transactions", new CreateSpendingTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Market",
            Amount = 80m,
            Source = new CreateSpendingTransactionSourceModel { AccountId = Guid.NewGuid() },
            Destinations = [new CreateSpendingTransactionDestinationModel
            {
                Location = new Models.Locations.LocationInputModel { NewLocationName = "Market" },
                Amount = 80m,
                FundAssignments = [new CreateFundAmountModel { FundId = groceries.Id, Amount = 80m }]
            }]
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>
    /// Returns consistent event facts for account, fund, and Fund Goal projections.
    /// </summary>
    [Fact]
    public async Task BalanceEventQueriesReturnPendingSpendingAcrossAllSurfaces()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(80m)
            .From(cash)
            .To("Market", groceries)
            .CreateAsync();

        CollectionModel<AccountBalanceEventModel> accountEvents = await test.Api.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{cash.Id}/balance-events?range.start=2026-07-01&range.end=2026-07-31");
        CollectionModel<FundBalanceEventModel> fundEvents = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            "/funds/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31");
        CollectionModel<FundGoalBalanceEventModel> goalEvents = await test.Api.GetAsync<CollectionModel<FundGoalBalanceEventModel>>(
            "/fund-goals/balance-events/date-range?range.start=2026-07-01&range.end=2026-07-31");

        Assert.Contains(accountEvents.Items, item => item.TransactionId == transaction.Id && !item.IsPosted && item.Amount == 80m);
        Assert.Contains(fundEvents.Items, item => item.TransactionId == transaction.Id && !item.IsPosted && item.Amount == 80m);
        Assert.Contains(goalEvents.Items, item => item.TransactionId == transaction.Id && !item.IsPosted && item.NewTotals.AmountSpentIncludingPending == 80m);
    }

    /// <summary>
    /// Refuses lifecycle operations that do not match the transaction state or identity.
    /// </summary>
    [Fact]
    public async Task MutationEndpointsRejectPostedAndMissingTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));

        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/transactions/{transaction.Id}");
        using HttpResponseMessage missingUnpost = await test.Api.PostResponseAsync($"/transactions/{Guid.NewGuid()}/unpost", new { });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, delete.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingUnpost.StatusCode);
    }

    /// <summary>
    /// Returns balance events for a valid accounting-period range.
    /// </summary>
    [Fact]
    public async Task BalanceEventAccountingPeriodRangeReturnsCreatedTransaction()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();

        CollectionModel<FundBalanceEventModel> events = await test.Api.GetAsync<CollectionModel<FundBalanceEventModel>>(
            $"/funds/balance-events/accounting-period-range?range.start={july.Id}&range.end={july.Id}&sort=DateDescending");

        Assert.Contains(events.Items, item => item.TransactionId == transaction.Id);
    }
}