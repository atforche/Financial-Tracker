using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Data;
using Microsoft.EntityFrameworkCore;

namespace Tests.AccountGoals;

/// <summary>
/// Verifies the Account Goal data repair included in the schema migration.
/// </summary>
public sealed class AccountGoalMigrationTests
{
    /// <summary>
    /// Backfills goals for standard accounts in applicable periods and excludes other account types.
    /// </summary>
    [Fact]
    public async Task MigrationBackfillsApplicableStandardAccountGoals()
    {
        await using MigrationTestDatabase database = await MigrationTestDatabase.CreateAsync();
        var july = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var august = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var openingAccount = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var onboardedAccount = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var creditAccount = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var laterOpeningAccount = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        await database.ExecuteAsync(
            """
            INSERT INTO "AccountingPeriods" ("Id", "Year", "Month", "Name", "IsOpen") VALUES
                ({0}, 2026, 7, 'July 2026', 1),
                ({1}, 2026, 8, 'August 2026', 1)
            """, july, august);
        await database.ExecuteAsync(
            """
            INSERT INTO "Accounts" ("Id", "Name", "Type", "OpeningAccountingPeriodId", "DateOpened", "OnboardedBalance") VALUES
                ({0}, 'Opening account', 'Standard', {1}, '2026-07-01', NULL),
                ({2}, 'Onboarded account', 'Standard', NULL, NULL, 100),
                ({3}, 'Credit account', 'CreditCard', NULL, NULL, 100),
                ({4}, 'Later account', 'Standard', {5}, '2026-08-01', NULL)
            """, openingAccount, july, onboardedAccount, creditAccount, laterOpeningAccount, august);

        await database.MigrateAsync();

        Assert.Equal(2, await database.CountAsync(openingAccount));
        Assert.Equal(2, await database.CountAsync(onboardedAccount));
        Assert.Equal(0, await database.CountAsync(creditAccount));
        Assert.Equal(1, await database.CountAsync(laterOpeningAccount));
        Assert.Equal(0, await database.CountAsync(laterOpeningAccount, july));
        Assert.Equal(0, await database.CountNonUppercaseIdsAsync());
    }

    /// <summary>
    /// Backfills a nullable-period onboarding goal when no accounting periods exist.
    /// </summary>
    [Fact]
    public async Task MigrationCreatesOnboardingGoalWhenNoPeriodsExist()
    {
        await using MigrationTestDatabase database = await MigrationTestDatabase.CreateAsync();
        var standardAccount = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

        await database.ExecuteAsync(
            "INSERT INTO \"Accounts\" (\"Id\", \"Name\", \"Type\", \"OpeningAccountingPeriodId\", \"DateOpened\", \"OnboardedBalance\") VALUES ({0}, 'Onboarded account', 'Standard', NULL, NULL, 100)",
            standardAccount);

        await database.MigrateAsync();

        Assert.Equal(1, await database.CountAsync(standardAccount, null));
        Assert.Equal(0, await database.CountNonUppercaseIdsAsync());
    }
}

internal sealed class MigrationTestDatabase : IAsyncDisposable
{
    private readonly string path;

    private MigrationTestDatabase(string path, DatabaseContext context)
    {
        this.path = path;
        Context = context;
    }

    public DatabaseContext Context { get; }

    public static async Task<MigrationTestDatabase> CreateAsync()
    {
        string path = Path.Combine(Path.GetTempPath(), $"financial-tracker-migration-tests-{Guid.NewGuid():N}.db");
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite($"Data Source={path}")
            .Options;
        DatabaseContext context = new(options);
        await context.Database.MigrateAsync("20260830133000_NormalizeUnassignedFundGoalIds");
        return new MigrationTestDatabase(path, context);
    }

    public Task MigrateAsync() => Context.Database.MigrateAsync();

    public Task ExecuteAsync(string sql, params object[] arguments) =>
        Context.Database.ExecuteSqlRawAsync(sql, arguments);

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
