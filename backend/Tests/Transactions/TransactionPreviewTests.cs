using System.Net;
using Models;
using Models.FundGoals;
using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;
using Models.Transactions.Update;

namespace Tests.Transactions;

/// <summary>
/// Covers previewing the Transaction a create, update, or posting would produce, without saving it.
/// </summary>
public sealed class TransactionPreviewTests
{
    /// <summary>
    /// A previewed fund transfer reports the posted balances it will have once created, and saves nothing.
    /// </summary>
    [Fact]
    public async Task PreviewCreateMatchesCreatedFundTransferWithoutSavingIt()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle unassigned = await test.FundQueries.GetUnassignedAsync(july);
        _ = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 5)).For(50m).From(unassigned).To(groceries).CreateAsync();
        TransactionHandle later = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 20)).For(100m).From(unassigned).To(groceries).CreateAsync();
        CreateTransactionModel request = test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 10)).For(200m).From(unassigned).To(groceries).CreateRequest();

        FundTransactionModel preview = await test.Api.PostAsync<CreateTransactionModel, FundTransactionModel>("/transactions/preview", request);
        CollectionModel<TransactionModel> afterPreview = await test.Api.GetAsync<CollectionModel<TransactionModel>>("/transactions");
        FundTransactionModel laterAfterPreview = await test.Api.GetAsync<FundTransactionModel>($"/transactions/{later.Id}");
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", request);
        FundTransactionModel saved = await test.Api.GetAsync<FundTransactionModel>($"/transactions/{created.Id}");

        Assert.Equal(2, afterPreview.TotalCount);
        Assert.Equal(950m, preview.Source.Fund.PreviousBalance?.PostedBalance);
        Assert.Equal(750m, preview.Source.Fund.NewBalance?.PostedBalance);
        Assert.Equal(50m, Assert.Single(preview.Destinations).Fund.PreviousBalance?.PostedBalance);
        Assert.Equal(250m, Assert.Single(preview.Destinations).Fund.NewBalance?.PostedBalance);
        Assert.Equal(50m, Assert.Single(laterAfterPreview.Destinations).Fund.PreviousBalance?.PostedBalance);
        AssertSameBalances(saved, preview);
    }

    /// <summary>
    /// A previewed fund transfer update reports the posted balances it will have once saved, and leaves the
    /// stored transfer unchanged.
    /// </summary>
    [Fact]
    public async Task PreviewUpdateMatchesUpdatedFundTransferWithoutSavingIt()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        FundHandle unassigned = await test.FundQueries.GetUnassignedAsync(july);
        _ = await test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 5)).For(50m).From(unassigned).To(groceries).CreateAsync();
        FundTransactionBuilder builder = test.Transactions.Fund().In(july).On(new DateOnly(2026, 7, 10)).For(200m).From(unassigned).To(groceries);
        TransactionHandle transfer = await builder.CreateAsync();
        UpdateTransactionModel request = builder.For(300m).UpdateRequest();

        FundTransactionModel preview = await test.Api.PostAsync<UpdateTransactionModel, FundTransactionModel>($"/transactions/{transfer.Id}/preview", request);
        FundTransactionModel afterPreview = await test.Api.GetAsync<FundTransactionModel>($"/transactions/{transfer.Id}");
        await test.Api.PostAsync($"/transactions/{transfer.Id}", request);
        FundTransactionModel saved = await test.Api.GetAsync<FundTransactionModel>($"/transactions/{transfer.Id}");

        Assert.Equal(650m, preview.Source.Fund.NewBalance?.PostedBalance);
        Assert.Equal(350m, Assert.Single(preview.Destinations).Fund.NewBalance?.PostedBalance);
        Assert.Equal(200m, afterPreview.Amount);
        Assert.Equal(250m, Assert.Single(afterPreview.Destinations).Fund.NewBalance?.PostedBalance);
        AssertSameBalances(saved, preview);
    }

    /// <summary>
    /// A previewed Transaction reports the posted balances it would have if posted on its own date, ignoring
    /// pending Transactions and later postings, exactly as it reports them once created and posted that day.
    /// </summary>
    [Fact]
    public async Task PreviewCreateMatchesTransactionPostedOnItsDateWithoutSavingIt()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle later = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 5)).For(50m).From(cash).To("Cafe", groceries).CreateAsync();
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 20));
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(200m).From(cash).To("Market", groceries).CreateAsync();
        CreateTransactionModel request = test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(100m).From(cash).To("Store", groceries).CreateRequest();

        SpendingTransactionModel preview = await test.Api.PostAsync<CreateTransactionModel, SpendingTransactionModel>("/transactions/preview", request);
        CollectionModel<TransactionModel> afterPreview = await test.Api.GetAsync<CollectionModel<TransactionModel>>("/transactions");
        CreateTransactionResultModel created = await test.Api.PostAsync<CreateTransactionModel, CreateTransactionResultModel>("/transactions", request);
        SpendingTransactionModel pending = await test.Api.GetAsync<SpendingTransactionModel>($"/transactions/{created.Id}");
        await test.Transactions.PostAsync(new TransactionHandle(created.Id), cash, new DateOnly(2026, 7, 15));
        SpendingTransactionModel saved = await test.Api.GetAsync<SpendingTransactionModel>($"/transactions/{created.Id}");

        Assert.Equal(2, afterPreview.TotalCount);
        Assert.Equal(1000m, preview.Source.Account.PreviousBalance?.PostedBalance);
        Assert.Equal(900m, preview.Source.Account.NewBalance?.PostedBalance);
        Assert.Equal(0m, Assert.Single(Assert.Single(preview.Destinations).FundAssignments).PreviousBalance?.PostedBalance);
        Assert.Equal(-100m, Assert.Single(Assert.Single(preview.Destinations).FundAssignments).NewBalance?.PostedBalance);
        Assert.False(pending.Source.Account.IsPosted);
        Assert.Null(pending.Source.Account.NewBalance);
        AssertSameBalances(saved, preview);
    }

    /// <summary>
    /// Previews have their own rate limit, so previewing does not use up the budget that real saves need.
    /// </summary>
    [Fact]
    public async Task PreviewsUseTheirOwnRateLimit()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        CreateTransactionModel request = test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(100m).From(cash).To("Store", groceries).CreateRequest();

        for (int previewCount = 0; previewCount < 60; previewCount++)
        {
            using HttpResponseMessage allowed = await test.Api.PostResponseAsync("/transactions/preview", request);
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        }
        using HttpResponseMessage limited = await test.Api.PostResponseAsync("/transactions/preview", request);
        using HttpResponseMessage created = await test.Api.PostResponseAsync("/transactions", request);

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
    }

    /// <summary>
    /// Previews report the same validation failures as the operations they preview.
    /// </summary>
    [Fact]
    public async Task PreviewsRejectInvalidRequests()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        SpendingTransactionBuilder spending = test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(200m).From(cash).To("Market", groceries);
        TransactionHandle transaction = await spending.CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 11));
        UpdateTransactionModel update = spending.For(75m).UpdateRequest();

        using HttpResponseMessage createResponse = await test.Api.PostResponseAsync("/transactions/preview",
            test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(100m).From(new AccountHandle(Guid.NewGuid(), "Unknown")).To("Store", groceries).CreateRequest());
        using HttpResponseMessage postedResponse = await test.Api.PostResponseAsync($"/transactions/{transaction.Id}/preview", update);
        using HttpResponseMessage missingResponse = await test.Api.PostResponseAsync($"/transactions/{Guid.NewGuid()}/preview", update);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, postedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingResponse.StatusCode);
    }

    /// <summary>
    /// A previewed posting reports the posted balances as of the posting date, ignoring pending Transactions
    /// and later postings, exactly as the Transaction reports them once posted, and saves nothing.
    /// </summary>
    [Fact]
    public async Task PreviewPostMatchesPostedTransactionWithoutSavingIt()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle later = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 5)).For(50m).From(cash).To("Cafe", groceries).CreateAsync();
        await test.Transactions.PostAsync(later, cash, new DateOnly(2026, 7, 20));
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 10)).For(200m).From(cash).To("Market", groceries).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(100m).From(cash).To("Bakery", groceries).CreateAsync();
        var request = new PostTransactionModel { AccountId = cash.Id, Date = new DateOnly(2026, 7, 16) };

        SpendingTransactionModel preview = await test.Api.PostAsync<PostTransactionModel, SpendingTransactionModel>($"/transactions/{transaction.Id}/post/preview", request);
        SpendingTransactionModel afterPreview = await test.Api.GetAsync<SpendingTransactionModel>($"/transactions/{transaction.Id}");
        SpendingTransactionModel laterAfterPreview = await test.Api.GetAsync<SpendingTransactionModel>($"/transactions/{later.Id}");
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 16));
        SpendingTransactionModel saved = await test.Api.GetAsync<SpendingTransactionModel>($"/transactions/{transaction.Id}");

        Assert.True(preview.Source.Account.IsPosted);
        Assert.Equal(1000m, preview.Source.Account.PreviousBalance?.PostedBalance);
        Assert.Equal(900m, preview.Source.Account.NewBalance?.PostedBalance);
        Assert.False(afterPreview.Source.Account.IsPosted);
        Assert.Equal(1000m, laterAfterPreview.Source.Account.PreviousBalance?.PostedBalance);
        AssertSameBalances(saved, preview);
    }

    /// <summary>
    /// Posting previews report the same validation failures as posting.
    /// </summary>
    [Fact]
    public async Task PreviewPostRejectsInvalidPostings()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(1000m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(100m).From(cash).To("Bakery", groceries).CreateAsync();

        using HttpResponseMessage earlyResponse = await test.Api.PostResponseAsync(
            $"/transactions/{transaction.Id}/post/preview",
            new PostTransactionModel { AccountId = cash.Id, Date = new DateOnly(2026, 7, 14) });
        using HttpResponseMessage unknownAccountResponse = await test.Api.PostResponseAsync(
            $"/transactions/{transaction.Id}/post/preview",
            new PostTransactionModel { AccountId = Guid.NewGuid(), Date = new DateOnly(2026, 7, 16) });
        SpendingTransactionModel afterPreviews = await test.Api.GetAsync<SpendingTransactionModel>($"/transactions/{transaction.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, earlyResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unknownAccountResponse.StatusCode);
        Assert.False(afterPreviews.Source.Account.IsPosted);
    }

    /// <summary>
    /// Asserts that two spending Transactions report the same account, fund, and Fund Goal balances.
    /// </summary>
    private static void AssertSameBalances(SpendingTransactionModel expected, SpendingTransactionModel actual)
    {
        Assert.Equal(expected.Source.Account.PreviousBalance?.PostedBalance, actual.Source.Account.PreviousBalance?.PostedBalance);
        Assert.Equal(expected.Source.Account.NewBalance?.PostedBalance, actual.Source.Account.NewBalance?.PostedBalance);
        SpendingTransactionDestinationModel expectedDestination = Assert.Single(expected.Destinations);
        SpendingTransactionDestinationModel actualDestination = Assert.Single(actual.Destinations);
        FundBalanceEventModel expectedFund = Assert.Single(expectedDestination.FundAssignments);
        FundBalanceEventModel actualFund = Assert.Single(actualDestination.FundAssignments);
        Assert.Equal(expectedFund.PreviousBalance?.PostedBalance, actualFund.PreviousBalance?.PostedBalance);
        Assert.Equal(expectedFund.NewBalance?.PostedBalance, actualFund.NewBalance?.PostedBalance);
        FundGoalBalanceEventModel expectedGoal = Assert.Single(expectedDestination.FundGoals);
        FundGoalBalanceEventModel actualGoal = Assert.Single(actualDestination.FundGoals);
        Assert.Equal(expectedGoal.PreviousTotals?.AmountSpent, actualGoal.PreviousTotals?.AmountSpent);
        Assert.Equal(expectedGoal.NewTotals?.AmountSpent, actualGoal.NewTotals?.AmountSpent);
    }

    /// <summary>
    /// Asserts that two fund transfers report the same fund balances.
    /// </summary>
    private static void AssertSameBalances(FundTransactionModel expected, FundTransactionModel actual)
    {
        Assert.Equal(expected.Source.Fund.PreviousBalance?.PostedBalance, actual.Source.Fund.PreviousBalance?.PostedBalance);
        Assert.Equal(expected.Source.Fund.NewBalance?.PostedBalance, actual.Source.Fund.NewBalance?.PostedBalance);
        FundBalanceEventModel expectedDestination = Assert.Single(expected.Destinations).Fund;
        FundBalanceEventModel actualDestination = Assert.Single(actual.Destinations).Fund;
        Assert.Equal(expectedDestination.PreviousBalance?.PostedBalance, actualDestination.PreviousBalance?.PostedBalance);
        Assert.Equal(expectedDestination.NewBalance?.PostedBalance, actualDestination.NewBalance?.PostedBalance);
    }
}
