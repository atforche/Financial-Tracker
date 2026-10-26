using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Tests.FundGoals;

/// <summary>
/// Verifies the data migrations that convert and repair Fund Goal configuration and contribution credit.
/// </summary>
public sealed class FundGoalMigrationTests
{
    /// <summary>
    /// Backfills missing minimums while preserving explicit bounds and optional contributions.
    /// </summary>
    [Fact]
    public async Task MigrationDefaultsMissingMinimumToZero()
    {
        await using MigrationTestDatabase database = await MigrationTestDatabase.CreateAsync(
            "20260831130000_MoveFundGoalContributionsWithFundTransfers");
        var defaultFundId = Guid.NewGuid();
        var reserveFundId = Guid.NewGuid();
        var defaultGoalId = Guid.NewGuid();
        var reserveGoalId = Guid.NewGuid();
        await database.ExecuteAsync(
            """
            INSERT INTO "Funds" ("Id", "Name", "Description", "OpeningAccountingPeriodId", "OnboardedBalance") VALUES
                ({0}, 'Default fund', '', NULL, 0),
                ({1}, 'Reserve fund', '', NULL, 0)
            """, defaultFundId, reserveFundId);
        await database.ExecuteAsync(
            """
            INSERT INTO "FundGoals" ("Id", "FundId", "AccountingPeriodId", "PlannedMonthlyContribution", "MinimumEndingBalance", "MaximumEndingBalance") VALUES
                ({0}, {1}, NULL, NULL, NULL, NULL),
                ({2}, {3}, NULL, 25, 100, 200)
            """, defaultGoalId, defaultFundId, reserveGoalId, reserveFundId);

        await database.MigrateAsync();

        Assert.Equal((0m, null), await GetBoundsAsync(database, defaultGoalId));
        Assert.Equal((100m, 200m), await GetBoundsAsync(database, reserveGoalId));
        Assert.Equal(1m, await database.ScalarDecimalAsync(
            "SELECT COUNT(*) FROM \"FundGoals\" WHERE \"Id\" = {0} AND \"PlannedMonthlyContribution\" IS NULL", defaultGoalId));
    }

    /// <summary>
    /// Converts funded bounds and gives a legacy exact ending target precedence.
    /// </summary>
    [Fact]
    public async Task MigrationConvertsLegacyBalanceGoals()
    {
        await using MigrationTestDatabase database = await MigrationTestDatabase.CreateAsync(
            "20260830170000_NormalizeAccountGoalIds");
        var rangedFundId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var exactFundId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var rangedGoalId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var exactGoalId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        await database.ExecuteAsync(
            """
            INSERT INTO "Funds" ("Id", "Name", "Description", "OpeningAccountingPeriodId", "OnboardedBalance") VALUES
                ({0}, 'Ranged fund', 'Ranged fund', NULL, 0),
                ({1}, 'Exact fund', 'Exact fund', NULL, 0)
            """, rangedFundId, exactFundId);
        await database.ExecuteAsync(
            """
            INSERT INTO "FundGoals" ("Id", "FundId", "AccountingPeriodId", "RegularContribution", "MinimumFundedBalance", "MaximumFundedBalance", "TargetEndingBalance") VALUES
                ({0}, {1}, NULL, 25, 100, 200, NULL),
                ({2}, {3}, NULL, 25, 100, 200, 150)
            """, rangedGoalId, rangedFundId, exactGoalId, exactFundId);

        await database.MigrateAsync();

        Assert.Equal((100m, 200m), await GetBoundsAsync(database, rangedGoalId));
        Assert.Equal((150m, 150m), await GetBoundsAsync(database, exactGoalId));
    }

    /// <summary>
    /// Moves existing contribution credit from a transfer source to its
    /// destination without changing the period aggregate.
    /// </summary>
    [Fact]
    public async Task MigrationMovesContributionCreditAcrossTransferGoals()
    {
        await using MigrationTestDatabase database = await MigrationTestDatabase.CreateAsync(
            "20260831120000_RenameFundGoalAmounts");
        var periodId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var balanceHistoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var sourceFundId = Guid.Parse("51a70ff9-49da-4463-88fd-818b17acf5c4");
        var destinationFundId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var transferId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var laterTransactionId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        await database.ExecuteAsync(
            """
            INSERT INTO "AccountingPeriods" ("Id", "Year", "Month", "Name", "IsOpen")
            VALUES ({0}, 2026, 7, 'July 2026', 1)
            """, periodId);
        await database.ExecuteAsync(
            """
            INSERT INTO "Funds" ("Id", "Name", "Description", "OpeningAccountingPeriodId", "OnboardedBalance") VALUES
                ({0}, 'Unassigned', 'Unassigned', {1}, NULL),
                ({2}, 'Phone', 'Phone', {1}, NULL)
            """, sourceFundId, periodId, destinationFundId);
        await database.ExecuteAsync(
            """
            INSERT INTO "AccountingPeriodBalanceHistories" ("Id", "AccountingPeriodId", "OpeningBalance", "ClosingBalance")
            VALUES ({0}, {1}, 0, 105)
            """, balanceHistoryId, periodId);
        await database.ExecuteAsync(
            """
            INSERT INTO "Transactions" (
                "Id", "Type", "AccountingPeriodId", "Date", "Sequence", "Description", "Amount",
                "FundTransaction_DebitFundId")
            VALUES ({0}, 1, {1}, '2026-07-11', 1, 'Phone top-up', 5, {2})
            """, transferId, periodId, sourceFundId);
        await database.ExecuteAsync(
            """
            INSERT INTO "FundTransactionDestinations" ("CreditFundId", "Amount", "FundTransactionId")
            VALUES ({0}, 5, {1})
            """, destinationFundId, transferId);
        await database.ExecuteAsync(
            """
            INSERT INTO "AccountingPeriodFundGoalTotals" (
                "Id", "FundId", "AccountingPeriodId", "AmountAssigned", "AmountSpent",
                "AmountAssignedToExpectedContribution", "AccountingPeriodBalanceHistoryId") VALUES
                ('66666666-6666-6666-6666-666666666666', {0}, {1}, 0, 0, 5, {2}),
                ('77777777-7777-7777-7777-777777777777', {3}, {1}, 105, 105, 100, {2})
            """, sourceFundId, periodId, balanceHistoryId, destinationFundId);
        await database.ExecuteAsync(
            """
            INSERT INTO "FundGoalTotalsHistories" (
                "Id", "FundId", "AccountingPeriodId", "TransactionId", "Date", "Sequence",
                "AmountAssigned", "AmountSpent", "AmountAssignedToExpectedContribution") VALUES
                ('80000000-0000-0000-0000-000000000001', {0}, {1}, '80000000-0000-0000-0000-000000000011', '2026-07-10', 1, 5, 0, 5),
                ('80000000-0000-0000-0000-000000000002', {2}, {1}, '80000000-0000-0000-0000-000000000012', '2026-07-10', 2, 100, 0, 100),
                ('80000000-0000-0000-0000-000000000003', {0}, {1}, {3}, '2026-07-11', 1, 0, 0, 5),
                ('80000000-0000-0000-0000-000000000004', {2}, {1}, {3}, '2026-07-11', 1, 105, 0, 100),
                ('80000000-0000-0000-0000-000000000005', {0}, {1}, {4}, '2026-07-12', 1, 0, 0, 5),
                ('80000000-0000-0000-0000-000000000006', {2}, {1}, {4}, '2026-07-12', 1, 105, 105, 100)
            """, sourceFundId, periodId, destinationFundId, transferId, laterTransactionId);
        await database.ExecuteAsync(
            """
            INSERT INTO "PendingFundGoalTotalsEffects" (
                "Id", "FundId", "AccountingPeriodId", "TransactionId", "PendingAmountAssigned",
                "PendingAmountSpent", "PendingAmountAssignedToExpectedContribution") VALUES
                ('90000000-0000-0000-0000-000000000001', {0}, {1}, {2}, -5, 0, 0),
                ('90000000-0000-0000-0000-000000000002', {3}, {1}, {2}, 5, 0, 0)
            """, sourceFundId, periodId, transferId, destinationFundId);

        await database.MigrateAsync();

        Assert.Equal(0m, await database.ScalarDecimalAsync(
            "SELECT \"AmountAssignedToExpectedContribution\" FROM \"AccountingPeriodFundGoalTotals\" WHERE \"FundId\" = {0}", sourceFundId));
        Assert.Equal(105m, await database.ScalarDecimalAsync(
            "SELECT \"AmountAssignedToExpectedContribution\" FROM \"AccountingPeriodFundGoalTotals\" WHERE \"FundId\" = {0}", destinationFundId));
        Assert.Equal(105m, await database.ScalarDecimalAsync(
            "SELECT SUM(\"AmountAssignedToExpectedContribution\") FROM \"AccountingPeriodFundGoalTotals\" WHERE \"AccountingPeriodId\" = {0}", periodId));
        Assert.Equal(5m, await database.ScalarDecimalAsync(
            "SELECT \"AmountAssignedToExpectedContribution\" FROM \"FundGoalTotalsHistories\" WHERE \"FundId\" = {0} AND \"Date\" = '2026-07-10'", sourceFundId));
        Assert.Equal(0m, await database.ScalarDecimalAsync(
            "SELECT \"AmountAssignedToExpectedContribution\" FROM \"FundGoalTotalsHistories\" WHERE \"FundId\" = {0} AND \"Date\" = '2026-07-12'", sourceFundId));
        Assert.Equal(105m, await database.ScalarDecimalAsync(
            "SELECT \"AmountAssignedToExpectedContribution\" FROM \"FundGoalTotalsHistories\" WHERE \"FundId\" = {0} AND \"Date\" = '2026-07-12'", destinationFundId));
    }

    private static async Task<(decimal? Minimum, decimal? Maximum)> GetBoundsAsync(
        MigrationTestDatabase database,
        Guid goalId)
    {
        await using System.Data.Common.DbCommand command = database.Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT \"MinimumEndingBalance\", \"MaximumEndingBalance\" FROM \"FundGoals\" WHERE \"Id\" = $goalId";
        System.Data.Common.DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = "$goalId";
        parameter.Value = goalId;
        _ = command.Parameters.Add(parameter);
        if (command.Connection!.State != System.Data.ConnectionState.Open)
        {
            await command.Connection.OpenAsync();
        }
        await using System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync();
        _ = await reader.ReadAsync();
        bool minimumIsNull = await reader.IsDBNullAsync(0);
        bool maximumIsNull = await reader.IsDBNullAsync(1);
        return (
            minimumIsNull ? null : Convert.ToDecimal(reader.GetValue(0), CultureInfo.InvariantCulture),
            maximumIsNull ? null : Convert.ToDecimal(reader.GetValue(1), CultureInfo.InvariantCulture));
    }
}
