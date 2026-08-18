using System.Net;
using Models.Transactions;
using Models.Transactions.Create;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Infrastructure;

namespace Tests.Transactions;

/// <summary>
/// Covers partial posting for a transaction affecting multiple destination accounts.
/// </summary>
public sealed class MultiDestinationAccountTransactionTests
{
    /// <summary>
    /// Posts each affected account independently while retaining pending effects for the rest.
    /// </summary>
    [Fact]
    public async Task PostAsyncKeepsUnpostedDestinationsPending()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle source = await test.Accounts.Onboard("Source").WithOpeningBalance(1000m).CreateAsync();
        AccountHandle firstDestination = await test.Accounts.Onboard("First").CreateAsync();
        AccountHandle secondDestination = await test.Accounts.Onboard("Second").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateAccountTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Split transfer",
            Amount = 100m,
            Source = new CreateAccountTransactionSourceModel { AccountId = source.Id },
            Destinations = [
                new CreateAccountTransactionDestinationModel { AccountId = firstDestination.Id, Amount = 40m },
                new CreateAccountTransactionDestinationModel { AccountId = secondDestination.Id, Amount = 60m }
            ]
        });
        TransactionHandle transaction = new(created.Id);

        await test.Transactions.PostAsync(transaction, source, new DateOnly(2026, 7, 20));

        AccountBalanceSnapshot sourceBalance = await test.AccountQueries.GetBalanceAsync(source);
        AccountBalanceSnapshot firstBalance = await test.AccountQueries.GetBalanceAsync(firstDestination);
        AccountBalanceSnapshot secondBalance = await test.AccountQueries.GetBalanceAsync(secondDestination);

        Assert.Equal(900m, sourceBalance.Posted);
        Assert.Equal(900m, sourceBalance.IncludingPending);
        Assert.Equal(0m, firstBalance.Posted);
        Assert.Equal(40m, firstBalance.IncludingPending);
        Assert.Equal(0m, secondBalance.Posted);
        Assert.Equal(60m, secondBalance.IncludingPending);

        await test.Transactions.PostAsync(transaction, firstDestination, new DateOnly(2026, 7, 21));
        await test.Transactions.PostAsync(transaction, secondDestination, new DateOnly(2026, 7, 22));

        firstBalance = await test.AccountQueries.GetBalanceAsync(firstDestination);
        secondBalance = await test.AccountQueries.GetBalanceAsync(secondDestination);
        Assert.Equal(40m, firstBalance.Posted);
        Assert.Equal(40m, firstBalance.IncludingPending);
        Assert.Equal(60m, secondBalance.Posted);
        Assert.Equal(60m, secondBalance.IncludingPending);

        await test.Transactions.UnpostAsync(transaction);
        sourceBalance = await test.AccountQueries.GetBalanceAsync(source);
        firstBalance = await test.AccountQueries.GetBalanceAsync(firstDestination);
        secondBalance = await test.AccountQueries.GetBalanceAsync(secondDestination);
        Assert.Equal(1000m, sourceBalance.Posted);
        Assert.Equal(900m, sourceBalance.IncludingPending);
        Assert.Equal(0m, firstBalance.Posted);
        Assert.Equal(40m, firstBalance.IncludingPending);
        Assert.Equal(0m, secondBalance.Posted);
        Assert.Equal(60m, secondBalance.IncludingPending);

        await test.Transactions.DeleteAsync(transaction);
        Assert.Equal(1000m, (await test.AccountQueries.GetBalanceAsync(source)).IncludingPending);
        Assert.Equal(0m, (await test.AccountQueries.GetBalanceAsync(firstDestination)).IncludingPending);
        Assert.Equal(0m, (await test.AccountQueries.GetBalanceAsync(secondDestination)).IncludingPending);
    }

    /// <summary>
    /// Prevents closing an Accounting Period until every affected Account has posted its transaction effect.
    /// </summary>
    [Fact]
    public async Task CloseAsyncRequiresEveryAffectedAccountToPost()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle source = await test.Accounts.Onboard("Source").WithOpeningBalance(100m).CreateAsync();
        AccountHandle destination = await test.Accounts.Onboard("Destination").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", new CreateAccountTransactionModel
        {
            AccountingPeriodId = july.Id,
            Date = new DateOnly(2026, 7, 15),
            Description = "Transfer",
            Amount = 20m,
            Source = new CreateAccountTransactionSourceModel { AccountId = source.Id },
            Destinations = [new CreateAccountTransactionDestinationModel { AccountId = destination.Id, Amount = 20m }]
        });
        TransactionHandle transaction = new(created.Id);

        await test.Transactions.PostAsync(transaction, source, new DateOnly(2026, 7, 15));
        using HttpResponseMessage partiallyPostedClose = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/close", new { });

        await test.Transactions.PostAsync(transaction, destination, new DateOnly(2026, 7, 15));
        using HttpResponseMessage fullyPostedClose = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/close", new { });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, partiallyPostedClose.StatusCode);
        Assert.Equal(HttpStatusCode.OK, fullyPostedClose.StatusCode);
    }
}
