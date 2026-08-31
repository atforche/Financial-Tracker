using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations;

/// <summary>
/// Reattributes Fund Goal contributions across the source and destinations of
/// existing Fund transactions without changing aggregate contributions.
/// </summary>
[DbContext(typeof(DatabaseContext))]
[Migration("20260831130000_MoveFundGoalContributionsWithFundTransfers")]
public partial class MoveFundGoalContributionsWithFundTransfers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "AccountingPeriodFundGoalTotals" AS totals
            SET "AmountAssignedToExpectedContribution" =
                totals."AmountAssignedToExpectedContribution"
                - COALESCE((
                    SELECT SUM(txn."Amount")
                    FROM "Transactions" AS txn
                    WHERE txn."AccountingPeriodId" = totals."AccountingPeriodId"
                        AND txn."FundTransaction_DebitFundId" = totals."FundId"
                ), 0)
                + COALESCE((
                    SELECT SUM(destination."Amount")
                    FROM "Transactions" AS txn
                    INNER JOIN "FundTransactionDestinations" AS destination
                        ON destination."FundTransactionId" = txn."Id"
                    WHERE txn."AccountingPeriodId" = totals."AccountingPeriodId"
                        AND destination."CreditFundId" = totals."FundId"
                ), 0);
            """);

        migrationBuilder.Sql("""
            UPDATE "FundGoalTotalsHistories" AS history
            SET "AmountAssignedToExpectedContribution" =
                history."AmountAssignedToExpectedContribution"
                - COALESCE((
                    SELECT SUM(txn."Amount")
                    FROM "Transactions" AS txn
                    WHERE txn."AccountingPeriodId" = history."AccountingPeriodId"
                        AND txn."FundTransaction_DebitFundId" = history."FundId"
                        AND (
                            txn."Date" < history."Date"
                            OR (txn."Date" = history."Date" AND txn."Sequence" <= history."Sequence")
                        )
                ), 0)
                + COALESCE((
                    SELECT SUM(destination."Amount")
                    FROM "Transactions" AS txn
                    INNER JOIN "FundTransactionDestinations" AS destination
                        ON destination."FundTransactionId" = txn."Id"
                    WHERE txn."AccountingPeriodId" = history."AccountingPeriodId"
                        AND destination."CreditFundId" = history."FundId"
                        AND (
                            txn."Date" < history."Date"
                            OR (txn."Date" = history."Date" AND txn."Sequence" <= history."Sequence")
                        )
                ), 0);
            """);

        migrationBuilder.Sql("""
            UPDATE "PendingFundGoalTotalsEffects" AS effect
            SET "PendingAmountAssignedToExpectedContribution" = effect."PendingAmountAssigned"
            WHERE EXISTS (
                SELECT 1
                FROM "Transactions" AS txn
                WHERE txn."Id" = effect."TransactionId"
                    AND txn."FundTransaction_DebitFundId" IS NOT NULL
            );
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This data repair is intentionally forward-only.
    }
}
