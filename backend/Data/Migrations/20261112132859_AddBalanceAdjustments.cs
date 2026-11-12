using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BalanceAdjustment_AccountId",
                table: "Transactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BalanceAdjustment_AccountId",
                table: "Transactions",
                column: "BalanceAdjustment_AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_BalanceAdjustment_AccountId",
                table: "Transactions",
                column: "BalanceAdjustment_AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Account transactions between these mirror locations and an Investment, Retirement, or Debt account
            // become Balance Adjustments (Type 5). Rocket Mortgage is also a real counterparty for escrow payments,
            // so its mirror transactions are converted but the location itself is kept.
            migrationBuilder.Sql("""
                CREATE TEMP TABLE "__MirrorLocations" ("Id" TEXT NOT NULL, "Deleted" INTEGER NOT NULL);
                INSERT INTO "__MirrorLocations" ("Id", "Deleted")
                SELECT "Id", "Name" <> 'Rocket Mortgage'
                FROM "Locations"
                WHERE "Name" IN ('CRI', 'Equitable', 'Fidelity', 'Voya', 'Rocket Mortgage');

                CREATE TEMP TABLE "__Conversions" (
                    "TransactionId" TEXT NOT NULL,
                    "AccountId" TEXT NOT NULL,
                    "IsDecrease" INTEGER NOT NULL,
                    "PostedDate" TEXT NULL,
                    "Date" TEXT NOT NULL);

                -- Location to account: a credit, which lowers the balance of a Debt account
                INSERT INTO "__Conversions"
                SELECT txn."Id", account."Id", account."Type" = 'Debt', destination."CreditPostedDate", txn."Date"
                FROM "Transactions" AS txn
                INNER JOIN "AccountTransactionDestinations" AS destination ON destination."AccountTransactionId" = txn."Id"
                INNER JOIN "Accounts" AS account ON account."Id" = destination."CreditAccountId"
                WHERE txn."Type" = 2
                    AND txn."AccountTransaction_SourceLocationId" IN (SELECT "Id" FROM "__MirrorLocations")
                    AND account."Type" IN ('Investment', 'Retirement', 'Debt');

                -- Account to location: a debit, which raises the balance of a Debt account
                INSERT INTO "__Conversions"
                SELECT txn."Id", account."Id", account."Type" <> 'Debt', txn."AccountTransaction_DebitPostedDate", txn."Date"
                FROM "Transactions" AS txn
                INNER JOIN "AccountTransactionDestinations" AS destination ON destination."AccountTransactionId" = txn."Id"
                INNER JOIN "Accounts" AS account ON account."Id" = txn."AccountTransaction_DebitAccountId"
                WHERE txn."Type" = 2
                    AND destination."LocationId" IN (SELECT "Id" FROM "__MirrorLocations")
                    AND account."Type" IN ('Investment', 'Retirement', 'Debt');

                -- Any row inserted here fails the migration, and the whole migration is rolled back
                CREATE TEMP TABLE "__UnexpectedMirrorLocationData" (
                    "Problem" TEXT NOT NULL,
                    CONSTRAINT "balance_adjustment_migration_found_unexpected_mirror_location_data" CHECK (0));

                INSERT INTO "__UnexpectedMirrorLocationData"
                SELECT 'Transaction has more than one destination: ' || "TransactionId"
                FROM "__Conversions"
                WHERE (SELECT COUNT(*) FROM "AccountTransactionDestinations" WHERE "AccountTransactionId" = "TransactionId") > 1
                UNION ALL
                SELECT 'Transaction is pending: ' || "TransactionId" FROM "__Conversions" WHERE "PostedDate" IS NULL
                UNION ALL
                SELECT 'Transaction is posted on a different date: ' || "TransactionId" FROM "__Conversions" WHERE "PostedDate" <> "Date"
                UNION ALL
                SELECT 'Location to delete is used by another transaction: ' || txn."Id"
                FROM "Transactions" AS txn
                WHERE txn."Id" NOT IN (SELECT "TransactionId" FROM "__Conversions")
                    AND (txn."AccountTransaction_SourceLocationId" IN (SELECT "Id" FROM "__MirrorLocations" WHERE "Deleted")
                        OR txn."IncomeTransaction_SourceLocationId" IN (SELECT "Id" FROM "__MirrorLocations" WHERE "Deleted")
                        OR EXISTS (SELECT 1 FROM "AccountTransactionDestinations" AS destination
                            WHERE destination."AccountTransactionId" = txn."Id"
                                AND destination."LocationId" IN (SELECT "Id" FROM "__MirrorLocations" WHERE "Deleted"))
                        OR EXISTS (SELECT 1 FROM "SpendingTransactionDestinations" AS destination
                            WHERE destination."SpendingTransactionId" = txn."Id"
                                AND destination."LocationId" IN (SELECT "Id" FROM "__MirrorLocations" WHERE "Deleted"))
                        OR EXISTS (SELECT 1 FROM "RefundTransactionSources" AS source
                            WHERE source."RefundTransactionId" = txn."Id"
                                AND source."LocationId" IN (SELECT "Id" FROM "__MirrorLocations" WHERE "Deleted")));

                DELETE FROM "AccountTransactionDestinations"
                WHERE "AccountTransactionId" IN (SELECT "TransactionId" FROM "__Conversions");

                UPDATE "Transactions" AS txn
                SET "Type" = 5,
                    "BalanceAdjustment_AccountId" = conversion."AccountId",
                    "Amount" = CASE WHEN conversion."IsDecrease" THEN '-' || txn."Amount" ELSE txn."Amount" END,
                    "AccountTransaction_DebitAccountId" = NULL,
                    "AccountTransaction_DebitPostedDate" = NULL,
                    "AccountTransaction_SourceLocationId" = NULL
                FROM "__Conversions" AS conversion
                WHERE conversion."TransactionId" = txn."Id";

                DELETE FROM "Locations" WHERE "Id" IN (SELECT "Id" FROM "__MirrorLocations" WHERE "Deleted");

                DROP TABLE "__UnexpectedMirrorLocationData";
                DROP TABLE "__Conversions";
                DROP TABLE "__MirrorLocations";
                """);
        }

        /// <inheritdoc />
        /// <remarks>
        /// The conversion of mirror location transactions and the deletion of those locations are not reversed.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_BalanceAdjustment_AccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BalanceAdjustment_AccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BalanceAdjustment_AccountId",
                table: "Transactions");
        }
    }
}
