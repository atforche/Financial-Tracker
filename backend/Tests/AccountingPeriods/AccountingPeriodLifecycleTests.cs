using System.Net;
using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.Funds;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.AccountingPeriods;

/// <summary>
/// Covers accounting-period sequencing and lifecycle constraints through the API.
/// </summary>
public sealed class AccountingPeriodLifecycleTests
{
    /// <summary>
    /// Rejects duplicate and noncontiguous periods while accepting the next period.
    /// </summary>
    [Fact]
    public async Task CreateAsyncRequiresUniqueContiguousPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage duplicate = await test.Api.PostResponseAsync("/accounting-periods", new CreateAccountingPeriodModel
        {
            Year = 2026,
            Month = 7,
        });
        using HttpResponseMessage gap = await test.Api.PostResponseAsync("/accounting-periods", new CreateAccountingPeriodModel
        {
            Year = 2026,
            Month = 9,
        });
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, gap.StatusCode);
        Assert.NotEqual(july.Id, august.Id);
    }

    /// <summary>
    /// Initializes the required Unassigned Fund when the first accounting period is created.
    /// </summary>
    [Fact]
    public async Task CreateFirstAsyncInitializesUnassignedFund()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Periods.Create(2026, 7).CreateAsync();

        CollectionModel<FundModel> funds = await test.Api.GetAsync<CollectionModel<FundModel>>("/funds");

        Assert.Contains(funds.Items, fund => fund.Name == "Unassigned");
    }

    /// <summary>
    /// Snapshots current posted Account and Fund balances when a later Accounting Period is created.
    /// </summary>
    [Fact]
    public async Task CreateAsyncCarriesPostedBalancesIntoTheNextPeriodOpening()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle income = await test.Funds.Create("Income").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Income()
            .In(july)
            .On(new DateOnly(2026, 7, 15))
            .For(40m)
            .From("Employer")
            .To(cash, income)
            .CreateAsync();
        await test.Transactions.PostAsync(transaction, cash, new DateOnly(2026, 7, 15));

        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        AccountsInAccountingPeriodRangeModel accounts = await test.Api.GetAsync<AccountsInAccountingPeriodRangeModel>(
            $"/accounts/accounting-period-range?range.start={july.Id}&range.end={august.Id}");
        FundsInAccountingPeriodRangeModel funds = await test.Api.GetAsync<FundsInAccountingPeriodRangeModel>(
            $"/funds/accounting-period-range?range.start={july.Id}&range.end={august.Id}");

        Assert.Equal(140m, Assert.Single(accounts.AccountingPeriods,
            item => item.AccountingPeriod.Id == august.Id).OpeningBalance.TotalBalance);
        Assert.Equal(40m, Assert.Single(funds.AccountingPeriods,
            item => item.AccountingPeriod.Id == august.Id).OpeningBalance.TotalAssignedBalance);
    }

    /// <summary>
    /// Allows a posted period to close and reopen, while preventing duplicate close operations.
    /// </summary>
    [Fact]
    public async Task CloseAndReopenAsyncEnforcePeriodState()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage close = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/close", new { });
        using HttpResponseMessage repeatClose = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/close", new { });
        using HttpResponseMessage reopen = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/reopen", new { });

        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, repeatClose.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reopen.StatusCode);
    }

    /// <summary>
    /// Requires later closed periods to reopen before an earlier closed period can reopen.
    /// </summary>
    [Fact]
    public async Task ReopenAsyncRequiresLaterClosedPeriodsToReopenFirst()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        await test.Api.PostAsync($"/accounting-periods/{july.Id}/close");
        await test.Api.PostAsync($"/accounting-periods/{august.Id}/close");

        using HttpResponseMessage reopenJulyFirst = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/reopen", new { });
        using HttpResponseMessage reopenAugust = await test.Api.PostResponseAsync($"/accounting-periods/{august.Id}/reopen", new { });
        using HttpResponseMessage reopenJuly = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/reopen", new { });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, reopenJulyFirst.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reopenAugust.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reopenJuly.StatusCode);
    }

    /// <summary>
    /// Refuses to close a period containing an unposted transaction.
    /// </summary>
    [Fact]
    public async Task CloseAsyncRejectsUnpostedTransactions()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        _ = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(10m).From(cash).To("Market", groceries).CreateAsync();

        using HttpResponseMessage response = await test.Api.PostResponseAsync($"/accounting-periods/{july.Id}/close", new { });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>
    /// Requires prior periods to close first and prevents deletion that would create a gap.
    /// </summary>
    [Fact]
    public async Task PeriodOrderingPreventsClosingLaterAndDeletingEarlierPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        using HttpResponseMessage closeLater = await test.Api.PostResponseAsync($"/accounting-periods/{august.Id}/close", new { });
        using HttpResponseMessage deleteEarlier = await test.Api.DeleteResponseAsync($"/accounting-periods/{july.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, closeLater.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, deleteEarlier.StatusCode);
    }

    /// <summary>
    /// Deletes the latest empty period and its period-owned unassigned fund.
    /// </summary>
    [Fact]
    public async Task DeleteAsyncRemovesTheLatestEmptyPeriodAndItsUnassignedFund()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        CollectionModel<FundModel> before = await test.Api.GetAsync<CollectionModel<FundModel>>("/funds");
        using HttpResponseMessage deleted = await test.Api.DeleteResponseAsync($"/accounting-periods/{july.Id}");
        using HttpResponseMessage missing = await test.Api.GetResponseAsync($"/accounting-periods/{july.Id}");
        CollectionModel<FundModel> after = await test.Api.GetAsync<CollectionModel<FundModel>>("/funds");

        Assert.Contains(before.Items, fund => fund.Name == "Unassigned");
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.DoesNotContain(after.Items, fund => fund.Name == "Unassigned");
    }

    /// <summary>
    /// Copies expected-income sources into the next period with payment dates cleared.
    /// </summary>
    [Fact]
    public async Task CreateAsyncCopiesExpectedIncomeSourcesWithoutDates()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        var source = new ExpectedIncomeSourceRequestModel
        {
            Name = "Employer",
            IncomeLines = [new Models.Transactions.Create.CreateIncomeLineModel { Description = "Salary", Amount = 1_000m }],
            IncomeDeductions = [new Models.Transactions.Create.CreateIncomeDeductionModel { Description = "Tax", Amount = 200m }],
            UntrackedTransfers = [new ExpectedUntrackedIncomeTransferRequestModel { Description = "Debt payment", Amount = 300m }],
            ExpectedDates = [new DateOnly(2026, 7, 15)],
        };

        AccountingPeriodWithBalanceModel updated = await test.Api.PostAsync<IReadOnlyCollection<ExpectedIncomeSourceRequestModel>, AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{july.Id}/expected-income-sources", [source]);
        Assert.Equal(800m, updated.ExpectedIncome.Total);
        Assert.Equal(500m, updated.ExpectedIncome.Tracked);
        Assert.Equal(300m, updated.ExpectedIncome.Untracked);
        ExpectedIncomeSourceModel updatedSource = Assert.Single(updated.ExpectedIncomeSources);
        Assert.Equal(800m, updatedSource.ExpectedAmount.Total);
        Assert.Equal(500m, updatedSource.ExpectedAmount.Tracked);
        Assert.Equal(300m, updatedSource.ExpectedAmount.Untracked);

        var changedSource = new ExpectedIncomeSourceRequestModel
        {
            Name = "Employer (updated)",
            IncomeLines = source.IncomeLines,
            IncomeDeductions = source.IncomeDeductions,
            UntrackedTransfers = source.UntrackedTransfers,
            ExpectedDates = [new DateOnly(2026, 7, 20)],
        };
        AccountingPeriodWithBalanceModel changed = await test.Api.PostAsync<IReadOnlyCollection<ExpectedIncomeSourceRequestModel>, AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{july.Id}/expected-income-sources", [changedSource]);
        ExpectedIncomeSourceModel changedResult = Assert.Single(changed.ExpectedIncomeSources);
        Assert.Equal("Employer (updated)", changedResult.Name);
        Assert.Equal(new DateOnly(2026, 7, 20), Assert.Single(changedResult.ExpectedDates));

        CollectionModel<AccountingPeriodWithBalanceModel> listed = await test.Api.GetAsync<CollectionModel<AccountingPeriodWithBalanceModel>>(
            "/accounting-periods/with-balances?filter.months=7");
        AccountingPeriodWithBalanceModel listedPeriod = Assert.Single(listed.Items);
        Assert.Equal(800m, listedPeriod.ExpectedIncome.Total);
        _ = Assert.Single(listedPeriod.ExpectedIncomeSources);

        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        AccountingPeriodWithBalanceModel result = await test.Api.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{august.Id}");

        ExpectedIncomeSourceModel copied = Assert.Single(result.ExpectedIncomeSources);
        Assert.Equal("Employer (updated)", copied.Name);
        Assert.Equal(800m, copied.NetAmount.Total);
        Assert.Equal(500m, copied.NetAmount.Tracked);
        Assert.Equal(300m, copied.NetAmount.Untracked);
        ExpectedUntrackedIncomeTransferModel copiedTransfer = Assert.Single(copied.UntrackedTransfers);
        Assert.Equal("Debt payment", copiedTransfer.Description);
        Assert.Equal(300m, copiedTransfer.Amount);
        Assert.Empty(copied.ExpectedDates);
    }

    /// <summary>
    /// Does not allow expected-income sources to change after their period closes.
    /// </summary>
    [Fact]
    public async Task ExpectedIncomeSourcesCannotBeUpdatedForClosedPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        await test.Api.PostAsync($"/accounting-periods/{july.Id}/close");

        using HttpResponseMessage response = await test.Api.PostResponseAsync(
            $"/accounting-periods/{july.Id}/expected-income-sources", Array.Empty<ExpectedIncomeSourceRequestModel>());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}