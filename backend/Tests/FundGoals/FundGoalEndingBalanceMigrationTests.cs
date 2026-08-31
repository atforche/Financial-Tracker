using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tests.AccountGoals;

namespace Tests.FundGoals;

/// <summary>
/// Verifies conversion of legacy Fund Goal balance configuration.
/// </summary>
public sealed class FundGoalEndingBalanceMigrationTests
{
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
