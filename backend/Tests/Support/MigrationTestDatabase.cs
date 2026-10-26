using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Data;
using Microsoft.EntityFrameworkCore;

namespace Tests.Support;

/// <summary>
/// A SQLite database migrated to a chosen migration, for seeding legacy data before migrating the rest of the way.
/// </summary>
internal sealed class MigrationTestDatabase : IAsyncDisposable
{
    private readonly string path;

    private MigrationTestDatabase(string path, DatabaseContext context)
    {
        this.path = path;
        Context = context;
    }

    public DatabaseContext Context { get; }

    public static async Task<MigrationTestDatabase> CreateAsync(
        string targetMigration = "20260830133000_NormalizeUnassignedFundGoalIds")
    {
        string path = Path.Combine(Path.GetTempPath(), $"financial-tracker-migration-tests-{Guid.NewGuid():N}.db");
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite($"Data Source={path}")
            .Options;
        DatabaseContext context = new(options);
        await context.Database.MigrateAsync(targetMigration);
        return new MigrationTestDatabase(path, context);
    }

    public Task MigrateAsync() => Context.Database.MigrateAsync();

    public Task ExecuteAsync(string sql, params object[] arguments) =>
        Context.Database.ExecuteSqlRawAsync(sql, arguments);

    [SuppressMessage("Security", "CA2100", Justification = "The command text is selected from fixed test SQL strings and contains only parameter placeholders.")]
    public async Task<decimal> ScalarDecimalAsync(string sql, params object[] arguments)
    {
        await using System.Data.Common.DbCommand command = Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = string.Format(CultureInfo.InvariantCulture, sql, arguments.Select((_, index) => $"$p{index}").ToArray());
        foreach ((object argument, int index) in arguments.Select((argument, index) => (argument, index)))
        {
            System.Data.Common.DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = $"$p{index}";
            parameter.Value = argument;
            _ = command.Parameters.Add(parameter);
        }
        if (Context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await Context.Database.OpenConnectionAsync();
        }
        return Convert.ToDecimal(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    public Task<int> CountAsync(Guid accountId) => CountAsync(accountId, null, false);

    public Task<int> CountAsync(Guid accountId, Guid? periodId) => CountAsync(accountId, periodId, true);

    public async Task<int> CountNonUppercaseIdsAsync()
    {
        await using System.Data.Common.DbCommand command = Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM \"AccountGoals\" WHERE \"Id\" != upper(\"Id\")";

        if (Context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await Context.Database.OpenConnectionAsync();
        }

        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    [SuppressMessage("Security", "CA2100", Justification = "The command text is selected from fixed SQL strings and contains only parameter placeholders.")]
    private async Task<int> CountAsync(Guid accountId, Guid? periodId, bool filterByPeriod)
    {
        await using System.Data.Common.DbCommand command = Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = !filterByPeriod
            ? "SELECT COUNT(*) FROM \"AccountGoals\" WHERE \"AccountId\" = $accountId"
            : periodId is null
            ? "SELECT COUNT(*) FROM \"AccountGoals\" WHERE \"AccountId\" = $accountId AND \"AccountingPeriodId\" IS NULL"
            : "SELECT COUNT(*) FROM \"AccountGoals\" WHERE \"AccountId\" = $accountId AND \"AccountingPeriodId\" = $periodId";
        System.Data.Common.DbParameter accountParameter = command.CreateParameter();
        accountParameter.ParameterName = "$accountId";
        accountParameter.Value = accountId;
        _ = command.Parameters.Add(accountParameter);
        if (filterByPeriod && periodId is not null)
        {
            System.Data.Common.DbParameter periodParameter = command.CreateParameter();
            periodParameter.ParameterName = "$periodId";
            periodParameter.Value = periodId.Value;
            _ = command.Parameters.Add(periodParameter);
        }

        if (Context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await Context.Database.OpenConnectionAsync();
        }

        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
