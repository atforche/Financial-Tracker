using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingPeriodEventBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AccountingPeriodBalanceChange",
                table: "FundBalanceHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingPeriodId",
                table: "FundBalanceHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE "FundBalanceHistories"
                SET "AccountingPeriodId" = (
                    SELECT "AccountingPeriodId" FROM "Transactions"
                    WHERE "Transactions"."Id" = "FundBalanceHistories"."TransactionId");
                """);

            migrationBuilder.AddColumn<decimal>(
                name: "AccountingPeriodBalanceChange",
                table: "AccountBalanceHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingPeriodId",
                table: "AccountBalanceHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE "AccountBalanceHistories"
                SET "AccountingPeriodId" = (
                    SELECT "AccountingPeriodId" FROM "Transactions"
                    WHERE "Transactions"."Id" = "AccountBalanceHistories"."TransactionId");
                """);

            // Balances are cents-only; integer cents keep the back-fill independent of floating-point sums.
            migrationBuilder.Sql("""
                WITH RECURSIVE
                global_order AS (
                    SELECT h."Id", h."AccountId", h."AccountingPeriodId", h."Date", h."Sequence",
                        CAST(ROUND(CAST(h."PostedBalance" AS REAL) * 100) AS INTEGER) - COALESCE(
                            LAG(CAST(ROUND(CAST(h."PostedBalance" AS REAL) * 100) AS INTEGER))
                                OVER (PARTITION BY h."AccountId" ORDER BY h."Date", h."Sequence"),
                            CAST(ROUND(CAST(COALESCE(a."OnboardedBalance", '0') AS REAL) * 100) AS INTEGER)) AS delta
                    FROM "AccountBalanceHistories" h
                    JOIN "Accounts" a ON a."Id" = h."AccountId"
                ),
                period_order AS (
                    SELECT *, ROW_NUMBER() OVER (
                        PARTITION BY "AccountId", "AccountingPeriodId" ORDER BY "Date", "Sequence") AS ordinal
                    FROM global_order
                ),
                running AS (
                    SELECT "Id", "AccountId", "AccountingPeriodId", ordinal, delta AS balance
                    FROM period_order WHERE ordinal = 1
                    UNION ALL
                    SELECT next."Id", next."AccountId", next."AccountingPeriodId", next.ordinal,
                        running.balance + next.delta
                    FROM running JOIN period_order next
                        ON next."AccountId" = running."AccountId"
                        AND next."AccountingPeriodId" = running."AccountingPeriodId"
                        AND next.ordinal = running.ordinal + 1
                )
                UPDATE "AccountBalanceHistories"
                SET "AccountingPeriodBalanceChange" = (
                    SELECT (CASE WHEN balance < 0 THEN '-' ELSE '' END)
                        || CAST(ABS(balance) / 100 AS TEXT) || '.' || printf('%02d', ABS(balance) % 100)
                    FROM running WHERE running."Id" = "AccountBalanceHistories"."Id");
                """);

            migrationBuilder.Sql("""
                WITH RECURSIVE
                global_order AS (
                    SELECT h."Id", h."FundId", h."AccountingPeriodId", h."Date", h."Sequence",
                        CAST(ROUND(CAST(h."PostedBalance" AS REAL) * 100) AS INTEGER) - COALESCE(
                            LAG(CAST(ROUND(CAST(h."PostedBalance" AS REAL) * 100) AS INTEGER))
                                OVER (PARTITION BY h."FundId" ORDER BY h."Date", h."Sequence"),
                            CAST(ROUND(CAST(COALESCE(f."OnboardedBalance", '0') AS REAL) * 100) AS INTEGER)) AS delta
                    FROM "FundBalanceHistories" h
                    JOIN "Funds" f ON f."Id" = h."FundId"
                ),
                period_order AS (
                    SELECT *, ROW_NUMBER() OVER (
                        PARTITION BY "FundId", "AccountingPeriodId" ORDER BY "Date", "Sequence") AS ordinal
                    FROM global_order
                ),
                running AS (
                    SELECT "Id", "FundId", "AccountingPeriodId", ordinal, delta AS balance
                    FROM period_order WHERE ordinal = 1
                    UNION ALL
                    SELECT next."Id", next."FundId", next."AccountingPeriodId", next.ordinal,
                        running.balance + next.delta
                    FROM running JOIN period_order next
                        ON next."FundId" = running."FundId"
                        AND next."AccountingPeriodId" = running."AccountingPeriodId"
                        AND next.ordinal = running.ordinal + 1
                )
                UPDATE "FundBalanceHistories"
                SET "AccountingPeriodBalanceChange" = (
                    SELECT (CASE WHEN balance < 0 THEN '-' ELSE '' END)
                        || CAST(ABS(balance) / 100 AS TEXT) || '.' || printf('%02d', ABS(balance) % 100)
                    FROM running WHERE running."Id" = "FundBalanceHistories"."Id");
                """);

            migrationBuilder.CreateIndex(
                name: "IX_FundBalanceHistories_FundId_AccountingPeriodId_Date_Sequence",
                table: "FundBalanceHistories",
                columns: new[] { "FundId", "AccountingPeriodId", "Date", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceHistories_AccountId_AccountingPeriodId_Date_Sequence",
                table: "AccountBalanceHistories",
                columns: new[] { "AccountId", "AccountingPeriodId", "Date", "Sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FundBalanceHistories_FundId_AccountingPeriodId_Date_Sequence",
                table: "FundBalanceHistories");

            migrationBuilder.DropIndex(
                name: "IX_AccountBalanceHistories_AccountId_AccountingPeriodId_Date_Sequence",
                table: "AccountBalanceHistories");

            migrationBuilder.DropColumn(
                name: "AccountingPeriodBalanceChange",
                table: "FundBalanceHistories");

            migrationBuilder.DropColumn(
                name: "AccountingPeriodId",
                table: "FundBalanceHistories");

            migrationBuilder.DropColumn(
                name: "AccountingPeriodBalanceChange",
                table: "AccountBalanceHistories");

            migrationBuilder.DropColumn(
                name: "AccountingPeriodId",
                table: "AccountBalanceHistories");
        }
    }
}
