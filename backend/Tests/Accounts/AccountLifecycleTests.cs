using System.Net;
using Models;
using Models.Accounts;

namespace Tests.Accounts;

/// <summary>
/// Covers creating, updating, and deleting accounts.
/// </summary>
public sealed class AccountLifecycleTests
{
    /// <summary>
    /// Creates an account of every type in an accounting period and exposes its type and zero balance.
    /// </summary>
    [Fact]
    public async Task CreateAsyncPersistsEveryAccountType()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        Dictionary<Guid, AccountTypeModel> types = [];
        foreach (AccountTypeModel type in Enum.GetValues<AccountTypeModel>())
        {
            AccountHandle account = await test.Accounts.Create(type.ToString(), july).WithType(type).CreateAsync();
            types.Add(account.Id, type);
        }

        CollectionModel<AccountWithBalanceModel> response = await test.Api.GetAsync<CollectionModel<AccountWithBalanceModel>>("/accounts/with-balances");

        Assert.All(response.Items, model => Assert.Equal(types[model.Id], model.Type));
        Assert.All(response.Items, model => Assert.Equal(0m, model.CurrentBalance.PostedBalance));
        Assert.Equal(types.Count, response.Items.Count);
    }

    /// <summary>
    /// Updates and deletes accounts while exposing missing-resource behavior.
    /// </summary>
    [Fact]
    public async Task UpdateAndDeleteAsyncReturnResourceContracts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle account = await test.Accounts.Onboard("Checking").WithOpeningBalance(200m).CreateAsync();

        AccountModel updated = await test.Api.PostAsync<UpdateAccountModel, AccountModel>($"/accounts/{account.Id}", new UpdateAccountModel { Name = "Main checking" });
        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/accounts/{account.Id}");
        using HttpResponseMessage missingUpdate = await test.Api.PostResponseAsync($"/accounts/{Guid.NewGuid()}", new UpdateAccountModel { Name = "Missing" });

        Assert.Equal("Main checking", updated.Name);
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingUpdate.StatusCode);
    }

    /// <summary>
    /// Rejects accounts opened outside, missing, or closed accounting periods, and onboarding after accounting has begun.
    /// </summary>
    [Fact]
    public async Task CreateAsyncEnforcesAccountingPeriodBoundaries()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        await test.Periods.CloseAsync(july);

        using HttpResponseMessage invalidDate = await test.Api.PostResponseAsync("/accounts", new CreateAccountModel
        {
            Name = "Invalid date",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = august.Id,
            DateOpened = new DateOnly(2026, 10, 1)
        });
        using HttpResponseMessage missingPeriod = await test.Api.PostResponseAsync("/accounts", new CreateAccountModel
        {
            Name = "Missing period",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = Guid.NewGuid(),
            DateOpened = new DateOnly(2026, 7, 1)
        });
        using HttpResponseMessage closedPeriod = await test.Api.PostResponseAsync("/accounts", new CreateAccountModel
        {
            Name = "Closed period",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1)
        });
        using HttpResponseMessage lateOnboarding = await test.Api.PostResponseAsync("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Late onboarding",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 10m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidDate.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingPeriod.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closedPeriod.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, lateOnboarding.StatusCode);
    }

    /// <summary>
    /// Keeps an account referenced by a transaction after a rejected deletion request.
    /// </summary>
    [Fact]
    public async Task DeleteAsyncRejectsAccountsReferencedByTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();

        using HttpResponseMessage delete = await test.Api.DeleteResponseAsync($"/accounts/{cash.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, delete.StatusCode);
        await test.AssertBalanceAsync(cash, 100m, 90m);
    }

    /// <summary>
    /// Adds an account to, and removes it from, every later accounting period's range.
    /// </summary>
    [Fact]
    public async Task CreateAndDeleteAsyncMaintainMembershipAcrossLaterPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        AccountHandle account = await test.Accounts.Create("Savings", july).CreateAsync();
        string rangeUri = $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}";

        Assert.Contains((await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(rangeUri)).Accounts.Items, item => item.Id == account.Id);

        await test.Api.DeleteAsync($"/accounts/{account.Id}");

        Assert.DoesNotContain((await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(rangeUri)).Accounts.Items, item => item.Id == account.Id);
    }
}
