using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations;

/// <summary>
/// Backfills period-scoped Unassigned Fund Goals and their totals data.
/// </summary>
[DbContext(typeof(DatabaseContext))]
[Migration("20260830120000_AddUnassignedFundGoals")]
public partial class AddUnassignedFundGoals : Migration
{
    private const string UnassignedFundId = "51A70FF9-49DA-4463-88FD-818B17ACF5C4";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($"""
            INSERT INTO "FundGoals" (
                "Id", "FundId", "AccountingPeriodId", "RegularContribution",
                "MinimumFundedBalance", "MaximumFundedBalance", "TargetEndingBalance")
            SELECT upper(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-'
                || hex(randomblob(2)) || '-' || hex(randomblob(2)) || '-'
                || hex(randomblob(6))), '{UnassignedFundId}', period."Id", NULL, NULL, NULL, NULL
            FROM "AccountingPeriods" AS period
            WHERE NOT EXISTS (
                SELECT 1
                FROM "FundGoals" AS goal
                WHERE goal."FundId" = '{UnassignedFundId}'
                    AND goal."AccountingPeriodId" = period."Id");
            """);

        migrationBuilder.Sql($"""
            WITH activity AS (
                SELECT txn."Id" AS "TransactionId", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    assignment."FundId", assignment."Amount" AS "AmountAssigned", 0 AS "AmountSpent",
                    CASE WHEN assignment."IsExtraContribution" = 0 THEN assignment."Amount" ELSE 0 END AS "RegularAmountAssigned",
                    CASE WHEN destination."PostedDate" IS NOT NULL THEN 1 ELSE 0 END AS "IsPosted"
                FROM "Transactions" AS txn
                INNER JOIN "IncomeTransactionIncomeDestinations" AS destination
                    ON destination."IncomeTransactionId" = txn."Id"
                INNER JOIN "IncomeTransactionIncomeDestinationFundAssignments" AS assignment
                    ON assignment."IncomeDestinationId" = destination."Id"
                WHERE assignment."FundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    '{UnassignedFundId}', -txn."Amount", 0, 0, 1
                FROM "Transactions" AS txn
                WHERE txn."FundTransaction_DebitFundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    destination."CreditFundId", 0, 0, 0, 1
                FROM "Transactions" AS txn
                INNER JOIN "FundTransactionDestinations" AS destination
                    ON destination."FundTransactionId" = txn."Id"
                WHERE destination."CreditFundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    '{UnassignedFundId}', 0, assignment."Amount", 0,
                    CASE WHEN (destination."CreditAccountId" IS NOT NULL AND destination."CreditPostedDate" IS NOT NULL)
                        OR (destination."CreditAccountId" IS NULL AND txn."SpendingTransaction_DebitPostedDate" IS NOT NULL)
                        THEN 1 ELSE 0 END
                FROM "Transactions" AS txn
                INNER JOIN "SpendingTransactionDestinations" AS destination
                    ON destination."SpendingTransactionId" = txn."Id"
                INNER JOIN "SpendingTransactionDestinationFundAssignments" AS assignment
                    ON assignment."DestinationId" = destination."Id"
                WHERE assignment."FundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    '{UnassignedFundId}', 0, -assignment."Amount", 0,
                    CASE WHEN (source."AccountId" IS NOT NULL AND source."PostedDate" IS NOT NULL)
                        OR (source."AccountId" IS NULL AND txn."RefundTransaction_DestinationPostedDate" IS NOT NULL)
                        THEN 1 ELSE 0 END
                FROM "Transactions" AS txn
                INNER JOIN "RefundTransactionSources" AS source
                    ON source."RefundTransactionId" = txn."Id"
                INNER JOIN "RefundTransactionSourceFundAssignments" AS assignment
                    ON assignment."SourceId" = source."Id"
                WHERE assignment."FundId" = '{UnassignedFundId}'
            ), posted_activity AS (
                SELECT * FROM activity WHERE "IsPosted" = 1
            ), grouped AS (
                SELECT "TransactionId", "AccountingPeriodId", "Date", "Sequence", "FundId",
                    SUM("AmountAssigned") AS "AmountAssigned",
                    SUM("AmountSpent") AS "AmountSpent",
                    SUM("RegularAmountAssigned") AS "RegularAmountAssigned"
                FROM posted_activity
                GROUP BY "TransactionId", "AccountingPeriodId", "Date", "Sequence", "FundId"
            )
            INSERT INTO "AccountingPeriodFundGoalTotals" (
                "Id", "FundId", "AccountingPeriodId", "AmountAssigned", "AmountSpent",
                "RegularAmountAssigned", "AccountingPeriodBalanceHistoryId")
            SELECT upper(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-'
                || hex(randomblob(2)) || '-' || hex(randomblob(2)) || '-'
                || hex(randomblob(6))), '{UnassignedFundId}', period."AccountingPeriodId",
                COALESCE((SELECT SUM(item."AmountAssigned") FROM posted_activity AS item
                    WHERE item."AccountingPeriodId" = period."AccountingPeriodId"), 0),
                COALESCE((SELECT SUM(item."AmountSpent") FROM posted_activity AS item
                    WHERE item."AccountingPeriodId" = period."AccountingPeriodId"), 0),
                COALESCE((SELECT SUM(item."RegularAmountAssigned") FROM posted_activity AS item
                    WHERE item."AccountingPeriodId" = period."AccountingPeriodId"), 0),
                period."Id"
            FROM "AccountingPeriodBalanceHistories" AS period
            WHERE NOT EXISTS (
                SELECT 1
                FROM "AccountingPeriodFundGoalTotals" AS totals
                WHERE totals."FundId" = '{UnassignedFundId}'
                    AND totals."AccountingPeriodId" = period."AccountingPeriodId");
            """);

        migrationBuilder.Sql($"""
            WITH activity AS (
                SELECT txn."Id" AS "TransactionId", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    assignment."FundId", assignment."Amount" AS "AmountAssigned", 0 AS "AmountSpent",
                    CASE WHEN assignment."IsExtraContribution" = 0 THEN assignment."Amount" ELSE 0 END AS "RegularAmountAssigned",
                    CASE WHEN destination."PostedDate" IS NOT NULL THEN 1 ELSE 0 END AS "IsPosted"
                FROM "Transactions" AS txn
                INNER JOIN "IncomeTransactionIncomeDestinations" AS destination
                    ON destination."IncomeTransactionId" = txn."Id"
                INNER JOIN "IncomeTransactionIncomeDestinationFundAssignments" AS assignment
                    ON assignment."IncomeDestinationId" = destination."Id"
                WHERE assignment."FundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    '{UnassignedFundId}', -txn."Amount", 0, 0, 1
                FROM "Transactions" AS txn
                WHERE txn."FundTransaction_DebitFundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    destination."CreditFundId", 0, 0, 0, 1
                FROM "Transactions" AS txn
                INNER JOIN "FundTransactionDestinations" AS destination
                    ON destination."FundTransactionId" = txn."Id"
                WHERE destination."CreditFundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    '{UnassignedFundId}', 0, assignment."Amount", 0,
                    CASE WHEN (destination."CreditAccountId" IS NOT NULL AND destination."CreditPostedDate" IS NOT NULL)
                        OR (destination."CreditAccountId" IS NULL AND txn."SpendingTransaction_DebitPostedDate" IS NOT NULL)
                        THEN 1 ELSE 0 END
                FROM "Transactions" AS txn
                INNER JOIN "SpendingTransactionDestinations" AS destination
                    ON destination."SpendingTransactionId" = txn."Id"
                INNER JOIN "SpendingTransactionDestinationFundAssignments" AS assignment
                    ON assignment."DestinationId" = destination."Id"
                WHERE assignment."FundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", txn."Date", txn."Sequence",
                    '{UnassignedFundId}', 0, -assignment."Amount", 0,
                    CASE WHEN (source."AccountId" IS NOT NULL AND source."PostedDate" IS NOT NULL)
                        OR (source."AccountId" IS NULL AND txn."RefundTransaction_DestinationPostedDate" IS NOT NULL)
                        THEN 1 ELSE 0 END
                FROM "Transactions" AS txn
                INNER JOIN "RefundTransactionSources" AS source
                    ON source."RefundTransactionId" = txn."Id"
                INNER JOIN "RefundTransactionSourceFundAssignments" AS assignment
                    ON assignment."SourceId" = source."Id"
                WHERE assignment."FundId" = '{UnassignedFundId}'
            ), posted_activity AS (
                SELECT * FROM activity WHERE "IsPosted" = 1
            ), grouped AS (
                SELECT "TransactionId", "AccountingPeriodId", "Date", "Sequence", "FundId",
                    SUM("AmountAssigned") AS "AmountAssigned",
                    SUM("AmountSpent") AS "AmountSpent",
                    SUM("RegularAmountAssigned") AS "RegularAmountAssigned"
                FROM posted_activity
                GROUP BY "TransactionId", "AccountingPeriodId", "Date", "Sequence", "FundId"
            )
            INSERT INTO "FundGoalTotalsHistories" (
                "Id", "FundId", "AccountingPeriodId", "TransactionId", "Date", "Sequence",
                "AmountAssigned", "AmountSpent", "RegularAmountAssigned")
            SELECT upper(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-'
                || hex(randomblob(2)) || '-' || hex(randomblob(2)) || '-'
                || hex(randomblob(6))), current."FundId", current."AccountingPeriodId",
                current."TransactionId", current."Date", current."Sequence",
                COALESCE((SELECT SUM(previous."AmountAssigned") FROM grouped AS previous
                    WHERE previous."FundId" = current."FundId"
                        AND previous."AccountingPeriodId" = current."AccountingPeriodId"
                        AND (previous."Date" < current."Date" OR
                            (previous."Date" = current."Date" AND previous."Sequence" <= current."Sequence"))), 0),
                COALESCE((SELECT SUM(previous."AmountSpent") FROM grouped AS previous
                    WHERE previous."FundId" = current."FundId"
                        AND previous."AccountingPeriodId" = current."AccountingPeriodId"
                        AND (previous."Date" < current."Date" OR
                            (previous."Date" = current."Date" AND previous."Sequence" <= current."Sequence"))), 0),
                COALESCE((SELECT SUM(previous."RegularAmountAssigned") FROM grouped AS previous
                    WHERE previous."FundId" = current."FundId"
                        AND previous."AccountingPeriodId" = current."AccountingPeriodId"
                        AND (previous."Date" < current."Date" OR
                            (previous."Date" = current."Date" AND previous."Sequence" <= current."Sequence"))), 0)
            FROM grouped AS current
            WHERE NOT EXISTS (
                SELECT 1
                FROM "FundGoalTotalsHistories" AS history
                WHERE history."FundId" = current."FundId"
                    AND history."AccountingPeriodId" = current."AccountingPeriodId"
                    AND history."TransactionId" = current."TransactionId");
            """);

        migrationBuilder.Sql($"""
            WITH activity AS (
                SELECT txn."Id" AS "TransactionId", txn."AccountingPeriodId", assignment."FundId",
                    assignment."Amount" AS "AmountAssigned", 0 AS "AmountSpent",
                    CASE WHEN assignment."IsExtraContribution" = 0 THEN assignment."Amount" ELSE 0 END AS "RegularAmountAssigned",
                    CASE WHEN destination."PostedDate" IS NULL THEN 1 ELSE 0 END AS "IsPending"
                FROM "Transactions" AS txn
                INNER JOIN "IncomeTransactionIncomeDestinations" AS destination
                    ON destination."IncomeTransactionId" = txn."Id"
                INNER JOIN "IncomeTransactionIncomeDestinationFundAssignments" AS assignment
                    ON assignment."IncomeDestinationId" = destination."Id"
                WHERE assignment."FundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", '{UnassignedFundId}', -txn."Amount", 0, 0, 1
                FROM "Transactions" AS txn
                WHERE txn."FundTransaction_DebitFundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", destination."CreditFundId", 0, 0, 0, 1
                FROM "Transactions" AS txn
                INNER JOIN "FundTransactionDestinations" AS destination
                    ON destination."FundTransactionId" = txn."Id"
                WHERE destination."CreditFundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", '{UnassignedFundId}', 0, assignment."Amount", 0,
                    CASE WHEN (destination."CreditAccountId" IS NOT NULL AND destination."CreditPostedDate" IS NULL)
                        OR (destination."CreditAccountId" IS NULL AND txn."SpendingTransaction_DebitPostedDate" IS NULL)
                        THEN 1 ELSE 0 END
                FROM "Transactions" AS txn
                INNER JOIN "SpendingTransactionDestinations" AS destination
                    ON destination."SpendingTransactionId" = txn."Id"
                INNER JOIN "SpendingTransactionDestinationFundAssignments" AS assignment
                    ON assignment."DestinationId" = destination."Id"
                WHERE assignment."FundId" = '{UnassignedFundId}'
                UNION ALL
                SELECT txn."Id", txn."AccountingPeriodId", '{UnassignedFundId}', 0, -assignment."Amount", 0,
                    CASE WHEN txn."RefundTransaction_DestinationPostedDate" IS NULL
                        OR (source."AccountId" IS NOT NULL AND source."PostedDate" IS NULL)
                        THEN 1 ELSE 0 END
                FROM "Transactions" AS txn
                INNER JOIN "RefundTransactionSources" AS source
                    ON source."RefundTransactionId" = txn."Id"
                INNER JOIN "RefundTransactionSourceFundAssignments" AS assignment
                    ON assignment."SourceId" = source."Id"
                WHERE assignment."FundId" = '{UnassignedFundId}'
            ), pending AS (
                SELECT "TransactionId", "AccountingPeriodId", "FundId",
                    SUM("AmountAssigned") AS "AmountAssigned",
                    SUM("AmountSpent") AS "AmountSpent",
                    SUM("RegularAmountAssigned") AS "RegularAmountAssigned"
                FROM activity
                WHERE "IsPending" = 1
                GROUP BY "TransactionId", "AccountingPeriodId", "FundId"
            )
            INSERT INTO "PendingFundGoalTotalsEffects" (
                "Id", "FundId", "AccountingPeriodId", "TransactionId", "PendingAmountAssigned",
                "PendingAmountSpent", "PendingRegularAmountAssigned")
            SELECT upper(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-'
                || hex(randomblob(2)) || '-' || hex(randomblob(2)) || '-'
                || hex(randomblob(6))), pending."FundId", pending."AccountingPeriodId",
                pending."TransactionId", pending."AmountAssigned", pending."AmountSpent",
                pending."RegularAmountAssigned"
            FROM pending
            WHERE NOT EXISTS (
                SELECT 1
                FROM "PendingFundGoalTotalsEffects" AS effect
                WHERE effect."FundId" = pending."FundId"
                    AND effect."AccountingPeriodId" = pending."AccountingPeriodId"
                    AND effect."TransactionId" = pending."TransactionId");
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This data backfill is intentionally forward-only.
    }
}
