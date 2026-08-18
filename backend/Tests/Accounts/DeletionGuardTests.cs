using System.Net;
using Tests.AccountingPeriods;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.Accounts;

/// <summary>
/// Covers deletion guards for resources referenced by Transactions.
/// </summary>
public sealed class DeletionGuardTests
{
    /// <summary>
    /// Keeps referenced Accounts and Funds available after rejected deletion requests.
    /// </summary>
    [Fact]
    public async Task DeleteAsyncRejectsResourcesReferencedByTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();

        using HttpResponseMessage accountDelete = await test.Api.DeleteResponseAsync($"/accounts/{cash.Id}");
        using HttpResponseMessage fundDelete = await test.Api.DeleteResponseAsync($"/funds/{groceries.Id}");
        using HttpResponseMessage accountRead = await test.Api.GetResponseAsync($"/accounts/{cash.Id}");
        using HttpResponseMessage fundRead = await test.Api.GetResponseAsync($"/funds/{groceries.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, accountDelete.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fundDelete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, accountRead.StatusCode);
        Assert.Equal(HttpStatusCode.OK, fundRead.StatusCode);
    }
}
