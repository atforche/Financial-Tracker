using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MinimumEndingBalance = table.Column<decimal>(type: "TEXT", nullable: true),
                    MaximumEndingBalance = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountGoals_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccountGoals_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Backfill only standard-account goals. Period-scoped IDs are derived from the
            // account and period IDs so the repair is deterministic if it is inspected or
            // replayed during development.
            migrationBuilder.Sql("""
                INSERT INTO "AccountGoals" ("Id", "AccountId", "AccountingPeriodId", "MinimumEndingBalance", "MaximumEndingBalance")
                SELECT lower(
                    substr(replace(account."Id", '-', ''), 1, 8) || '-' ||
                    substr(replace(account."Id", '-', ''), 9, 4) || '-' ||
                    substr(replace(account."Id", '-', ''), 13, 4) || '-' ||
                    substr(replace(period."Id", '-', ''), 1, 4) || '-' ||
                    substr(replace(period."Id", '-', ''), 5, 12)),
                    account."Id",
                    period."Id",
                    NULL,
                    NULL
                FROM "Accounts" AS account
                CROSS JOIN "AccountingPeriods" AS period
                WHERE account."Type" = 'Standard'
                  AND (
                      account."OpeningAccountingPeriodId" IS NULL
                      OR EXISTS (
                          SELECT 1
                          FROM "AccountingPeriods" AS opening_period
                          WHERE opening_period."Id" = account."OpeningAccountingPeriodId"
                            AND (period."Year" * 12 + period."Month") >=
                                (opening_period."Year" * 12 + opening_period."Month")
                      )
                  )
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "AccountGoals" AS existing_goal
                      WHERE existing_goal."AccountId" = account."Id"
                        AND existing_goal."AccountingPeriodId" = period."Id"
                  );

                INSERT INTO "AccountGoals" ("Id", "AccountId", "AccountingPeriodId", "MinimumEndingBalance", "MaximumEndingBalance")
                SELECT lower(account."Id"), account."Id", NULL, NULL, NULL
                FROM "Accounts" AS account
                WHERE account."Type" = 'Standard'
                  AND NOT EXISTS (SELECT 1 FROM "AccountingPeriods")
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "AccountGoals" AS existing_goal
                      WHERE existing_goal."AccountId" = account."Id"
                        AND existing_goal."AccountingPeriodId" IS NULL
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AccountGoals_AccountId",
                table: "AccountGoals",
                column: "AccountId",
                unique: true,
                filter: "\"AccountingPeriodId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountGoals_AccountId_AccountingPeriodId",
                table: "AccountGoals",
                columns: new[] { "AccountId", "AccountingPeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountGoals_AccountingPeriodId",
                table: "AccountGoals",
                column: "AccountingPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountGoals");
        }
    }
}
