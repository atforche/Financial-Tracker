namespace Tests.AccountGoals;

/// <summary>
/// Verifies the Account Goal data repair included in the schema migration.
/// </summary>
public sealed class AccountGoalMigrationTests
{
    /// <summary>
    /// Backfills missing minimums while preserving explicit bounds.
    /// </summary>
    [Fact]
    public async Task MigrationDefaultsMissingMinimumToZero()
    {
        await using MigrationTestDatabase database = await MigrationTestDatabase.CreateAsync(
            "20260906004755_DefaultFundGoalMinimumEndingBalance");
        var defaultAccountId = Guid.NewGuid();
        var boundedAccountId = Guid.NewGuid();
        var defaultGoalId = Guid.NewGuid();
        var boundedGoalId = Guid.NewGuid();
        await database.ExecuteAsync(
            """
            INSERT INTO "Accounts" ("Id", "Name", "Type", "OpeningAccountingPeriodId", "DateOpened", "OnboardedBalance") VALUES
                ({0}, 'Checking', 'Standard', NULL, NULL, 100),
                ({1}, 'Savings', 'Standard', NULL, NULL, 100)
            """, defaultAccountId, boundedAccountId);
        await database.ExecuteAsync(
            """
            INSERT INTO "AccountGoals" ("Id", "AccountId", "AccountingPeriodId", "MinimumEndingBalance", "MaximumEndingBalance") VALUES
                ({0}, {1}, NULL, NULL, NULL),
                ({2}, {3}, NULL, 50, 200)
            """, defaultGoalId, defaultAccountId, boundedGoalId, boundedAccountId);

        await database.MigrateAsync();

        Assert.Equal(0m, await database.ScalarDecimalAsync(
            "SELECT \"MinimumEndingBalance\" FROM \"AccountGoals\" WHERE \"Id\" = {0}", defaultGoalId));
        Assert.Equal(50m, await database.ScalarDecimalAsync(
            "SELECT \"MinimumEndingBalance\" FROM \"AccountGoals\" WHERE \"Id\" = {0}", boundedGoalId));
        Assert.Equal(200m, await database.ScalarDecimalAsync(
            "SELECT \"MaximumEndingBalance\" FROM \"AccountGoals\" WHERE \"Id\" = {0}", boundedGoalId));
    }

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
