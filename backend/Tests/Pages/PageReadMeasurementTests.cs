using System.Diagnostics;
using Models;
using Models.AccountGoals;
using Models.Locations;
using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;
using Xunit.Abstractions;

namespace Tests.Pages;

/// <summary>
/// Records repeatable API-side read costs for a small, fixed financial fixture.
/// </summary>
public sealed class PageReadMeasurementTests(ITestOutputHelper output)
{
    /// <summary>
    /// Measures request count, JSON bytes, initialized SQL commands, and summed request time by page read group.
    /// </summary>
    [Fact]
    public async Task FixedFixturePageReadCostsAreRecorded()
    {
        ReadQueryCounter counter = new();
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync(counter);
        AccountHandle cash = await test.Accounts.Onboard("Cash").WithFinancialInstitution("Bank").WithOpeningBalance(100m).CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle income = await test.Transactions.Income().In(july).On(new DateOnly(2026, 7, 10)).For(40m).From("Employer").To(cash, groceries).CreateAsync();
        TransactionHandle spending = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(20m).From(cash).To("Market", groceries).CreateAsync();
        await test.Transactions.PostAsync(income, cash, new DateOnly(2026, 7, 10));
        await test.Transactions.PostAsync(spending, cash, new DateOnly(2026, 7, 15));
        CollectionModel<LocationModel> locations = await test.Api.GetAsync<CollectionModel<LocationModel>>("/locations");
        LocationModel market = Assert.Single(locations.Items, item => item.Name == "Market");
        AccountGoalModel accountGoal = await test.Api.GetAsync<AccountGoalModel>(
            $"/account-goals/account/{cash.Id}?accountingPeriodId={july.Id}");

        string dateRange = "range.start=2026-07-01&range.end=2026-07-31";
        string periodRange = $"range.start={july.Id}&range.end={july.Id}";
        (string Page, string[] Paths)[] groups =
        [
            ("Overview", ["/accounting-periods?limit=500", $"/accounts/date-range?{dateRange}&limit=1", $"/funds/date-range?{dateRange}&limit=1"]),
            ("Account workspace", ["/accounting-periods?limit=500", "/accounts/with-balances", "/accounts/financial-institutions"]),
            ("Account detail", ["/accounts/with-balances", $"/accounts/{cash.Id}/balance-events?limit=5", "/accounts/financial-institutions", $"/accounts/{cash.Id}/balance-events/totals"]),
            ("Account trends", ["/accounting-periods?limit=500", $"/accounts/date-range?{dateRange}&limit=5"]),
            ("Fund workspace", ["/accounting-periods?limit=500", "/funds/with-balances"]),
            ("Fund create", ["/accounting-periods/with-balances?limit=500"]),
            ("Fund detail", ["/funds/with-balances", $"/funds/balance-events/date-range?{dateRange}&filter.names=Groceries&limit=5", $"/funds/balance-events/date-range/totals?{dateRange}&filter.names=Groceries"]),
            ("Fund trends", ["/accounting-periods?limit=500", $"/funds/date-range?{dateRange}&limit=5"]),
            ("Period workspace", ["/accounting-periods?sort=Date&limit=1", "/accounting-periods/with-balances?sort=DateDescending&limit=1", "/accounting-periods/with-balances?limit=5"]),
            ("Period Cash Flow", [$"/accounting-periods/{july.Id}", $"/accounting-periods/{july.Id}/transactions?limit=5", $"/accounts/accounting-period-range?{periodRange}&limit=1", $"/funds/accounting-period-range?{periodRange}&limit=1"]),
            ("Period Plan", [$"/accounting-periods/{july.Id}", $"/fund-goals?filter.accountingPeriodIds={july.Id}", $"/fund-goals/progress/{july.Id}", $"/account-goals?filter.accountingPeriodIds={july.Id}", $"/account-goals/progress/{july.Id}"]),
            ("Period trends", ["/accounting-periods?limit=500", $"/accounting-periods/range?{periodRange}&limit=5", $"/accounts/accounting-period-range?{periodRange}&limit=1", $"/funds/accounting-period-range?{periodRange}&limit=1"]),
            ("Expected income source", [$"/accounting-periods/{july.Id}"]),
            ("Account goal workspace", ["/accounting-periods?limit=500", $"/account-goals?filter.accountingPeriodIds={july.Id}", $"/account-goals/progress/{july.Id}"]),
            ("Account goal trends", ["/accounting-periods?limit=500", $"/account-goals?filter.accountingPeriodIds={july.Id}", $"/account-goals/progress/{july.Id}"]),
            ("Account goal detail", ["/accounting-periods?limit=500", $"/account-goals/account/{cash.Id}?accountingPeriodId={july.Id}", $"/account-goals/{accountGoal.Id}/progress/{july.Id}", $"/accounts/{cash.Id}/balance-events?limit=5", $"/accounts/{cash.Id}/balance-events/totals"]),
            ("Fund goal workspace", ["/accounting-periods?limit=500", $"/fund-goals?filter.accountingPeriodIds={july.Id}", $"/fund-goals/progress/{july.Id}"]),
            ("Fund goal trends", ["/accounting-periods?limit=500", $"/fund-goals?filter.accountingPeriodIds={july.Id}", $"/fund-goals/progress/{july.Id}"]),
            ("Fund goal detail", ["/accounting-periods?limit=500", $"/fund-goals/fund/{groceries.Id}?accountingPeriodId={july.Id}", $"/fund-goals/{groceries.Goal.Id}/progress/{july.Id}", $"/fund-goals/balance-events/accounting-period-range?{periodRange}&filter.fundIds={groceries.Id}&limit=5", $"/fund-goals/balance-events/accounting-period-range/totals?{periodRange}&filter.fundIds={groceries.Id}"]),
            ("Location workspace", ["/locations?limit=500"]),
            ("Location trends", ["/locations?limit=500", "/accounting-periods?limit=500", $"/transactions/date-range?{dateRange}&filter.locationIds={market.Id}&limit=5"]),
            ("Transaction workspace filtered", ["/accounting-periods?limit=500", "/accounts/with-balances", "/funds/with-balances", "/locations?limit=500", $"/fund-goals?filter.accountingPeriodIds={july.Id}&limit=500", $"/fund-goals/progress/{july.Id}", $"/transactions?filter.accountingPeriodIds={july.Id}&limit=5"]),
            ("Transaction create", ["/accounting-periods?limit=500", "/accounts/with-balances", "/funds/with-balances", "/locations?limit=500", $"/fund-goals?filter.accountingPeriodIds={july.Id}&limit=500", $"/fund-goals/progress/{july.Id}"]),
            ("Transaction detail", [$"/transactions/{spending.Id}", $"/accounting-periods/{july.Id}", "/funds/with-balances", $"/fund-goals?filter.accountingPeriodIds={july.Id}&limit=500", $"/fund-goals/progress/{july.Id}"]),
            ("Transaction edit", [$"/transactions/{spending.Id}", "/accounting-periods?limit=500", "/accounts/with-balances", "/funds/with-balances", "/locations?limit=500", $"/fund-goals?filter.accountingPeriodIds={july.Id}&limit=500", $"/fund-goals/progress/{july.Id}"]),
            ("Location detail", ["/locations", $"/transactions?filter.locationIds={market.Id}&limit=5", $"/transactions/date-range?{dateRange}&filter.locationIds={market.Id}&limit=500"]),
            ("User administration", ["/users/me", "/users", "/user-invitations"]),
        ];

        output.WriteLine("Fixture: one period, one account, one fund, two posted transactions");
        await MeasureAsync(test.Api, counter, groups);
    }

    /// <summary>
    /// Measures the most expensive page reads with several periods, accounts, funds, and transactions.
    /// </summary>
    [Fact]
    public async Task ExpandedFixtureReadCostsAreRecorded()
    {
        ReadQueryCounter counter = new();
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync(counter);
        List<AccountHandle> accounts = [];
        for (int index = 1; index <= 3; index++)
        {
            accounts.Add(await test.Accounts.Onboard($"Account {index}").WithOpeningBalance(1000m).CreateAsync());
        }
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        List<FundHandle> funds = [];
        for (int index = 1; index <= 3; index++)
        {
            funds.Add(await test.Funds.Create($"Fund {index}").In(july).CreateAsync());
        }
        List<AccountingPeriodHandle> periods = [july];
        for (int month = 8; month <= 9; month++)
        {
            periods.Add(await test.Periods.Create(2026, month).CreateAsync());
        }
        for (int periodIndex = 0; periodIndex < periods.Count; periodIndex++)
        {
            AccountingPeriodHandle period = periods[periodIndex];
            DateOnly transactionDate = new(2026, periodIndex + 7, 15);
            for (int index = 0; index < accounts.Count; index++)
            {
                TransactionHandle spending = await test.Transactions.Spending().In(period)
                    .On(transactionDate).For(10m)
                    .From(accounts[index]).To("Market", funds[index]).CreateAsync();
                await test.Transactions.PostAsync(spending, accounts[index], transactionDate);
            }
        }
        CollectionModel<LocationModel> locations = await test.Api.GetAsync<CollectionModel<LocationModel>>("/locations");
        LocationModel market = Assert.Single(locations.Items, item => item.Name == "Market");

        string dateRange = "range.start=2026-07-01&range.end=2026-09-30";
        string periodRange = $"range.start={july.Id}&range.end={periods[^1].Id}";
        Guid accountId = accounts[0].Id;
        (string Page, string[] Paths)[] groups =
        [
            ("Overview", ["/accounting-periods?limit=500", $"/accounts/date-range?{dateRange}&limit=1", $"/funds/date-range?{dateRange}&limit=1"]),
            ("Account workspace", ["/accounting-periods?limit=500", "/accounts/with-balances", "/accounts/financial-institutions"]),
            ("Account detail", ["/accounts/with-balances", $"/accounts/{accountId}/balance-events?limit=5", "/accounts/financial-institutions", $"/accounts/{accountId}/balance-events/totals"]),
            ("Fund detail", ["/funds/with-balances", $"/funds/balance-events/date-range?{dateRange}&filter.names=Fund%201&limit=5", $"/funds/balance-events/date-range/totals?{dateRange}&filter.names=Fund%201"]),
            ("Period Cash Flow", [$"/accounting-periods/{periods[^1].Id}", $"/accounting-periods/{periods[^1].Id}/transactions?limit=5", $"/accounts/accounting-period-range?{periodRange}&limit=1", $"/funds/accounting-period-range?{periodRange}&limit=1"]),
            ("Period Plan", [$"/accounting-periods/{periods[^1].Id}", $"/fund-goals?filter.accountingPeriodIds={periods[^1].Id}", $"/fund-goals/progress/{periods[^1].Id}", $"/account-goals?filter.accountingPeriodIds={periods[^1].Id}", $"/account-goals/progress/{periods[^1].Id}"]),
            ("Transaction workspace filtered", ["/accounting-periods?limit=500", "/accounts/with-balances", "/funds/with-balances", "/locations?limit=500", $"/fund-goals?filter.accountingPeriodIds={periods[^1].Id}&limit=500", $"/fund-goals/progress/{periods[^1].Id}", $"/transactions?filter.accountingPeriodIds={periods[^1].Id}&limit=5"]),
            ("Location detail", ["/locations", $"/transactions?filter.locationIds={market.Id}&limit=5", $"/transactions/date-range?{dateRange}&filter.locationIds={market.Id}&limit=500"]),
        ];

        output.WriteLine("Fixture: three periods, three accounts, three funds, nine posted transactions");
        await MeasureAsync(test.Api, counter, groups);
    }

    private async Task MeasureAsync(TestApiClient api, ReadQueryCounter counter, IEnumerable<(string Page, string[] Paths)> groups)
    {
        output.WriteLine("Page | Requests | JSON bytes | Initialized SQL commands | Summed in-process ms");
        foreach ((string page, string[] paths) in groups)
        {
            counter.Reset();
            long bytes = 0;
            long elapsedMilliseconds = 0;
            foreach (string path in paths)
            {
                var watch = Stopwatch.StartNew();
                using HttpResponseMessage response = await api.GetResponseAsync(path);
                byte[] body = await response.Content.ReadAsByteArrayAsync();
                watch.Stop();
                _ = response.EnsureSuccessStatusCode();
                bytes += body.Length;
                elapsedMilliseconds += watch.ElapsedMilliseconds;
            }
            Assert.True(bytes > 0);
            Assert.True(counter.Count > 0);
            output.WriteLine($"{page} | {paths.Length} | {bytes} | {counter.Count} | {elapsedMilliseconds}");
        }
    }
}
