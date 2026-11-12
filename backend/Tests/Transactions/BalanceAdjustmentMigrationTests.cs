using Microsoft.Data.Sqlite;

namespace Tests.Transactions;

/// <summary>
/// Tests for the migration that converts mirror location Account transactions into Balance Adjustments.
/// </summary>
public sealed class BalanceAdjustmentMigrationTests
{
    private const string PreviousMigration = "20261029123115_RemoveExtraContributionFlag";

    private static readonly Guid PeriodId = Guid.Parse("10000000-0000-0000-0000-000000000000");
    private static readonly Guid RetirementId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid LoanId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid EscrowId = Guid.Parse("20000000-0000-0000-0000-000000000003");
    private static readonly Guid FidelityId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid CriId = Guid.Parse("30000000-0000-0000-0000-000000000002");
    private static readonly Guid RocketMortgageId = Guid.Parse("30000000-0000-0000-0000-000000000003");
    private static readonly Guid EmployerId = Guid.Parse("30000000-0000-0000-0000-000000000004");
    private static readonly Guid IncreaseId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private static readonly Guid DecreaseId = Guid.Parse("40000000-0000-0000-0000-000000000002");
    private static readonly Guid InterestId = Guid.Parse("40000000-0000-0000-0000-000000000003");
    private static readonly Guid MortgageInterestId = Guid.Parse("40000000-0000-0000-0000-000000000004");
    private static readonly Guid EscrowPaymentId = Guid.Parse("40000000-0000-0000-0000-000000000005");
    private static readonly Guid ContributionId = Guid.Parse("40000000-0000-0000-0000-000000000006");

    /// <summary>
    /// Mirror transactions become signed Balance Adjustments, and only the mirror-only locations are deleted.
    /// </summary>
    [Fact]
    public async Task MigrationConvertsMirrorTransactionsAndDeletesMirrorLocations()
    {
        await using MigrationTestDatabase database = await MigrationTestDatabase.CreateAsync(PreviousMigration);
        await SeedAsync(database);

        await database.MigrateAsync();

        // Location to a Retirement account raised its balance; Retirement account to location lowered it
        await AssertAdjustmentAsync(database, IncreaseId, RetirementId, 100m);
        await AssertAdjustmentAsync(database, DecreaseId, RetirementId, -40m);
        // Debt account to location raised the amount owed
        await AssertAdjustmentAsync(database, InterestId, LoanId, 25m);
        await AssertAdjustmentAsync(database, MortgageInterestId, LoanId, 30m);
        // Real counterparties stay Account transactions
        Assert.Equal(2m, await database.ScalarDecimalAsync("SELECT \"Type\" FROM \"Transactions\" WHERE \"Id\" = {0}", EscrowPaymentId));
        Assert.Equal(2m, await database.ScalarDecimalAsync("SELECT \"Type\" FROM \"Transactions\" WHERE \"Id\" = {0}", ContributionId));
        Assert.Equal(2m, await database.ScalarDecimalAsync("SELECT COUNT(*) FROM \"AccountTransactionDestinations\""));
        Assert.Equal(0m, await database.ScalarDecimalAsync(
            "SELECT COUNT(*) FROM \"Locations\" WHERE \"Id\" IN ({0}, {1})", FidelityId, CriId));
        Assert.Equal(2m, await database.ScalarDecimalAsync(
            "SELECT COUNT(*) FROM \"Locations\" WHERE \"Id\" IN ({0}, {1})", RocketMortgageId, EmployerId));
    }

    /// <summary>
    /// A pending mirror transaction fails the whole migration rather than being skipped.
    /// </summary>
    [Fact]
    public async Task MigrationFailsWhenMirrorTransactionIsPending()
    {
        await using MigrationTestDatabase database = await MigrationTestDatabase.CreateAsync(PreviousMigration);
        await SeedAsync(database);
        await database.ExecuteAsync(
            "UPDATE \"AccountTransactionDestinations\" SET \"CreditPostedDate\" = NULL WHERE \"AccountTransactionId\" = {0}", IncreaseId);

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(database.MigrateAsync);

        Assert.Contains("balance_adjustment_migration_found_unexpected_mirror_location_data", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0m, await database.ScalarDecimalAsync("SELECT COUNT(*) FROM \"Transactions\" WHERE \"Type\" = 5"));
        Assert.Equal(4m, await database.ScalarDecimalAsync("SELECT COUNT(*) FROM \"Locations\""));
    }

    private static async Task SeedAsync(MigrationTestDatabase database)
    {
        await database.ExecuteAsync(
            """
            INSERT INTO "AccountingPeriods" ("Id", "Year", "Month", "Name", "IsOpen") VALUES ({0}, 2026, 7, 'July 2026', 1)
            """, PeriodId);
        await database.ExecuteAsync(
            """
            INSERT INTO "Accounts" ("Id", "Name", "Type", "OpeningAccountingPeriodId", "DateOpened", "OnboardedBalance") VALUES
                ({0}, '401k', 'Retirement', NULL, NULL, 0),
                ({1}, 'Loan', 'Debt', NULL, NULL, 0),
                ({2}, 'Escrow', 'Escrow', NULL, NULL, 0)
            """, RetirementId, LoanId, EscrowId);
        await database.ExecuteAsync(
            """
            INSERT INTO "Locations" ("Id", "Name", "NormalizedName") VALUES
                ({0}, 'Fidelity', 'FIDELITY'),
                ({1}, 'CRI', 'CRI'),
                ({2}, 'Rocket Mortgage', 'ROCKET MORTGAGE'),
                ({3}, 'Saline Area Schools', 'SALINE AREA SCHOOLS')
            """, FidelityId, CriId, RocketMortgageId, EmployerId);
        await InsertFromLocationAsync(database, IncreaseId, 1, FidelityId, RetirementId, "100");
        await InsertToLocationAsync(database, DecreaseId, 2, RetirementId, FidelityId, "40");
        await InsertToLocationAsync(database, InterestId, 3, LoanId, CriId, "25");
        await InsertToLocationAsync(database, MortgageInterestId, 4, LoanId, RocketMortgageId, "30");
        await InsertToLocationAsync(database, EscrowPaymentId, 5, EscrowId, RocketMortgageId, "50");
        await InsertFromLocationAsync(database, ContributionId, 6, EmployerId, RetirementId, "10");
    }

    private static async Task InsertFromLocationAsync(
        MigrationTestDatabase database, Guid transactionId, int sequence, Guid locationId, Guid accountId, string amount)
    {
        await database.ExecuteAsync(
            """
            INSERT INTO "Transactions" ("Id", "AccountingPeriodId", "Amount", "Date", "Description", "Sequence", "Type", "AccountTransaction_SourceLocationId")
            VALUES ({0}, {1}, {2}, '2026-07-15', 'Change in value', {3}, 2, {4})
            """, transactionId, PeriodId, amount, sequence, locationId);
        await database.ExecuteAsync(
            """
            INSERT INTO "AccountTransactionDestinations" ("AccountTransactionId", "Amount", "CreditAccountId", "CreditPostedDate", "LocationId")
            VALUES ({0}, {1}, {2}, '2026-07-15', NULL)
            """, transactionId, amount, accountId);
    }

    private static async Task InsertToLocationAsync(
        MigrationTestDatabase database, Guid transactionId, int sequence, Guid accountId, Guid locationId, string amount)
    {
        await database.ExecuteAsync(
            """
            INSERT INTO "Transactions" ("Id", "AccountingPeriodId", "Amount", "Date", "Description", "Sequence", "Type", "AccountTransaction_DebitAccountId", "AccountTransaction_DebitPostedDate")
            VALUES ({0}, {1}, {2}, '2026-07-15', 'Change in value', {3}, 2, {4}, '2026-07-15')
            """, transactionId, PeriodId, amount, sequence, accountId);
        await database.ExecuteAsync(
            """
            INSERT INTO "AccountTransactionDestinations" ("AccountTransactionId", "Amount", "CreditAccountId", "CreditPostedDate", "LocationId")
            VALUES ({0}, {1}, NULL, NULL, {2})
            """, transactionId, amount, locationId);
    }

    private static async Task AssertAdjustmentAsync(MigrationTestDatabase database, Guid transactionId, Guid accountId, decimal amount)
    {
        Assert.Equal(1m, await database.ScalarDecimalAsync(
            """
            SELECT COUNT(*) FROM "Transactions"
            WHERE "Id" = {0} AND "Type" = 5 AND "BalanceAdjustment_AccountId" = {1}
                AND "AccountTransaction_DebitAccountId" IS NULL AND "AccountTransaction_SourceLocationId" IS NULL
            """, transactionId, accountId));
        Assert.Equal(amount, await database.ScalarDecimalAsync("SELECT \"Amount\" FROM \"Transactions\" WHERE \"Id\" = {0}", transactionId));
    }
}
