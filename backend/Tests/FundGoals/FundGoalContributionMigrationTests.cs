using Tests.AccountGoals;

namespace Tests.FundGoals;

/// <summary>
/// Verifies the Fund Goal contribution reattribution data repair.
/// </summary>
public sealed class FundGoalContributionMigrationTests
{
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
        Assert.Equal(-5m, await database.ScalarDecimalAsync(
            "SELECT \"PendingAmountAssignedToExpectedContribution\" FROM \"PendingFundGoalTotalsEffects\" WHERE \"FundId\" = {0}", sourceFundId));
        Assert.Equal(5m, await database.ScalarDecimalAsync(
            "SELECT \"PendingAmountAssignedToExpectedContribution\" FROM \"PendingFundGoalTotalsEffects\" WHERE \"FundId\" = {0}", destinationFundId));
    }
}
