using Tests.AccountGoals;

namespace Tests.Transactions;

/// <summary>Checks cent-exact back-fill of period-scoped balances from existing posted histories.</summary>
public sealed class AccountingPeriodBalanceMigrationTests
{
    /// <summary>Interleaved periods preserve cents and independent running changes.</summary>
    [Fact]
    public async Task MigrationBackfillsInterleavedHistoriesExactly()
    {
        await using MigrationTestDatabase database = await MigrationTestDatabase.CreateAsync(
            "20260907000000_AddFundGoalContributionMaximumOverride");
        var july = Guid.NewGuid();
        var august = Guid.NewGuid();
        var account = Guid.NewGuid();
        var fund = Guid.NewGuid();
        Guid[] transactions = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        Guid[] accountHistories = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        Guid[] fundHistories = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        await database.ExecuteAsync(
            """
            INSERT INTO "AccountingPeriods" ("Id", "Name", "Year", "Month", "IsOpen") VALUES
                ({0}, 'July', 2026, 7, 1), ({1}, 'August', 2026, 8, 1)
            """, july, august);
        await database.ExecuteAsync(
            "INSERT INTO \"Accounts\" (\"Id\", \"Name\", \"Type\", \"OnboardedBalance\") VALUES ({0}, 'Cash', 'Asset', {1})",
            account, 100.12m);
        await database.ExecuteAsync(
            "INSERT INTO \"Funds\" (\"Id\", \"Name\", \"Description\", \"OnboardedBalance\") VALUES ({0}, 'Reserve', '', {1})",
            fund, 10.12m);
        for (int index = 0; index < transactions.Length; index++)
        {
            await database.ExecuteAsync(
                "INSERT INTO \"Transactions\" (\"Id\", \"Type\", \"AccountingPeriodId\", \"Date\", \"Sequence\", \"Description\", \"Amount\") VALUES ({0}, 0, {1}, {2}, {3}, 'Legacy', 1)",
                transactions[index], index == 1 ? august : july, $"2026-07-{10 + index:00}", index + 1);
            await database.ExecuteAsync(
                "INSERT INTO \"AccountBalanceHistories\" (\"Id\", \"AccountId\", \"TransactionId\", \"Date\", \"Sequence\", \"PostedBalance\") VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                accountHistories[index], account, transactions[index], $"2026-07-{10 + index:00}", index + 1,
                index == 0 ? 101.23m : index == 1 ? 102.34m : 99.17m);
            await database.ExecuteAsync(
                "INSERT INTO \"FundBalanceHistories\" (\"Id\", \"FundId\", \"TransactionId\", \"Date\", \"Sequence\", \"PostedBalance\") VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                fundHistories[index], fund, transactions[index], $"2026-07-{10 + index:00}", index + 1,
                index == 0 ? 11.23m : index == 1 ? 12.34m : 9.17m);
        }

        await database.MigrateAsync();

        Assert.Equal(1.11m, await GetChangeAsync(database, "AccountBalanceHistories", accountHistories[0], july));
        Assert.Equal(1.11m, await GetChangeAsync(database, "AccountBalanceHistories", accountHistories[1], august));
        Assert.Equal(-2.06m, await GetChangeAsync(database, "AccountBalanceHistories", accountHistories[2], july));
        Assert.Equal(1.11m, await GetChangeAsync(database, "FundBalanceHistories", fundHistories[0], july));
        Assert.Equal(1.11m, await GetChangeAsync(database, "FundBalanceHistories", fundHistories[1], august));
        Assert.Equal(-2.06m, await GetChangeAsync(database, "FundBalanceHistories", fundHistories[2], july));
    }

    private static Task<decimal> GetChangeAsync(MigrationTestDatabase database, string table, Guid historyId, Guid periodId) =>
        database.ScalarDecimalAsync(
            table == "AccountBalanceHistories"
                ? "SELECT \"AccountingPeriodBalanceChange\" FROM \"AccountBalanceHistories\" WHERE \"Id\" = {0} AND \"AccountingPeriodId\" = {1}"
                : "SELECT \"AccountingPeriodBalanceChange\" FROM \"FundBalanceHistories\" WHERE \"Id\" = {0} AND \"AccountingPeriodId\" = {1}",
            historyId, periodId);
}
