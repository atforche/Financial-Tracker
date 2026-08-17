using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IncomeTransaction_SourceLocation",
                table: "Transactions",
                newName: "IncomeTransaction_SourceLocationId");

            migrationBuilder.RenameColumn(
                name: "AccountTransaction_SourceLocation",
                table: "Transactions",
                newName: "AccountTransaction_SourceLocationId");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "SpendingTransactionDestinations",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "AccountTransactionDestinations",
                newName: "LocationId");

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.Sql("""
                WITH RECURSIVE
                "RawLocations"("Name") AS (
                    SELECT "IncomeTransaction_SourceLocationId" FROM "Transactions"
                    WHERE "IncomeTransaction_SourceLocationId" IS NOT NULL AND trim("IncomeTransaction_SourceLocationId") <> ''
                    UNION ALL
                    SELECT "AccountTransaction_SourceLocationId" FROM "Transactions"
                    WHERE "AccountTransaction_SourceLocationId" IS NOT NULL AND trim("AccountTransaction_SourceLocationId") <> ''
                    UNION ALL
                    SELECT "LocationId" FROM "SpendingTransactionDestinations"
                    WHERE "LocationId" IS NOT NULL AND trim("LocationId") <> ''
                    UNION ALL
                    SELECT "LocationId" FROM "AccountTransactionDestinations"
                    WHERE "LocationId" IS NOT NULL AND trim("LocationId") <> ''
                ),
                "NormalizedLocations"("OriginalName", "Name") AS (
                    SELECT "Name", trim(replace(replace(replace("Name", char(9), ' '), char(10), ' '), char(13), ' '))
                    FROM "RawLocations"
                    UNION ALL
                    SELECT "OriginalName", replace("Name", '  ', ' ')
                    FROM "NormalizedLocations"
                    WHERE instr("Name", '  ') > 0
                ),
                "FinalLocations"("Name", "NormalizedName") AS (
                    SELECT "Name", upper("Name")
                    FROM "NormalizedLocations"
                    WHERE instr("Name", '  ') = 0
                ),
                "Spellings"("Name", "NormalizedName", "UsageCount") AS (
                    SELECT "Name", "NormalizedName", count(*)
                    FROM "FinalLocations"
                    GROUP BY "Name", "NormalizedName"
                ),
                "RankedSpellings"("Name", "NormalizedName", "Rank") AS (
                    SELECT "Name", "NormalizedName",
                        row_number() OVER (PARTITION BY "NormalizedName" ORDER BY "UsageCount" DESC, "Name")
                    FROM "Spellings"
                )
                INSERT INTO "Locations" ("Id", "Name", "NormalizedName")
                SELECT lower(
                    hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' || substr(hex(randomblob(2)), 2) || '-' ||
                    substr('89ab', abs(random()) % 4 + 1, 1) || substr(hex(randomblob(2)), 2) || '-' || hex(randomblob(6))),
                    "Name",
                    "NormalizedName"
                FROM "RankedSpellings"
                WHERE "Rank" = 1;
                """);

            migrationBuilder.Sql(NormalizeLegacyLocation("Transactions", "IncomeTransaction_SourceLocationId"));
            migrationBuilder.Sql(NormalizeLegacyLocation("Transactions", "AccountTransaction_SourceLocationId"));
            migrationBuilder.Sql(NormalizeLegacyLocation("SpendingTransactionDestinations", "LocationId"));
            migrationBuilder.Sql(NormalizeLegacyLocation("AccountTransactionDestinations", "LocationId"));

            migrationBuilder.Sql("""
                UPDATE "AccountTransactionDestinations" AS "Keeper"
                SET "Amount" = (
                    SELECT sum("Duplicate"."Amount")
                    FROM "AccountTransactionDestinations" AS "Duplicate"
                    WHERE "Duplicate"."AccountTransactionId" = "Keeper"."AccountTransactionId"
                        AND "Duplicate"."LocationId" = "Keeper"."LocationId")
                WHERE "Keeper"."LocationId" IS NOT NULL
                    AND "Keeper"."Id" = (
                        SELECT min("Candidate"."Id")
                        FROM "AccountTransactionDestinations" AS "Candidate"
                        WHERE "Candidate"."AccountTransactionId" = "Keeper"."AccountTransactionId"
                            AND "Candidate"."LocationId" = "Keeper"."LocationId");

                DELETE FROM "AccountTransactionDestinations" AS "Duplicate"
                WHERE "Duplicate"."LocationId" IS NOT NULL
                    AND "Duplicate"."Id" <> (
                        SELECT min("Candidate"."Id")
                        FROM "AccountTransactionDestinations" AS "Candidate"
                        WHERE "Candidate"."AccountTransactionId" = "Duplicate"."AccountTransactionId"
                            AND "Candidate"."LocationId" = "Duplicate"."LocationId");
                """);

            migrationBuilder.Sql("""
                UPDATE "SpendingTransactionDestinations" AS "Keeper"
                SET "Amount" = (
                    SELECT sum("Duplicate"."Amount")
                    FROM "SpendingTransactionDestinations" AS "Duplicate"
                    WHERE "Duplicate"."SpendingTransactionId" = "Keeper"."SpendingTransactionId"
                        AND "Duplicate"."LocationId" = "Keeper"."LocationId")
                WHERE "Keeper"."LocationId" IS NOT NULL
                    AND "Keeper"."Id" = (
                        SELECT min("Candidate"."Id")
                        FROM "SpendingTransactionDestinations" AS "Candidate"
                        WHERE "Candidate"."SpendingTransactionId" = "Keeper"."SpendingTransactionId"
                            AND "Candidate"."LocationId" = "Keeper"."LocationId");

                UPDATE "SpendingTransactionDestinationFundAssignments" AS "Assignment"
                SET "DestinationId" = (
                    SELECT min("Candidate"."Id")
                    FROM "SpendingTransactionDestinations" AS "Current"
                    INNER JOIN "SpendingTransactionDestinations" AS "Candidate"
                        ON "Candidate"."SpendingTransactionId" = "Current"."SpendingTransactionId"
                        AND "Candidate"."LocationId" = "Current"."LocationId"
                    WHERE "Current"."Id" = "Assignment"."DestinationId")
                WHERE EXISTS (
                    SELECT 1 FROM "SpendingTransactionDestinations" AS "Current"
                    WHERE "Current"."Id" = "Assignment"."DestinationId"
                        AND "Current"."LocationId" IS NOT NULL);

                UPDATE "SpendingTransactionDestinationFundAssignments" AS "Keeper"
                SET "Amount" = (
                    SELECT sum("Duplicate"."Amount")
                    FROM "SpendingTransactionDestinationFundAssignments" AS "Duplicate"
                    WHERE "Duplicate"."DestinationId" = "Keeper"."DestinationId"
                        AND "Duplicate"."FundId" = "Keeper"."FundId")
                WHERE "Keeper"."Id" = (
                    SELECT min("Candidate"."Id")
                    FROM "SpendingTransactionDestinationFundAssignments" AS "Candidate"
                    WHERE "Candidate"."DestinationId" = "Keeper"."DestinationId"
                        AND "Candidate"."FundId" = "Keeper"."FundId");

                DELETE FROM "SpendingTransactionDestinationFundAssignments" AS "Duplicate"
                WHERE "Duplicate"."Id" <> (
                    SELECT min("Candidate"."Id")
                    FROM "SpendingTransactionDestinationFundAssignments" AS "Candidate"
                    WHERE "Candidate"."DestinationId" = "Duplicate"."DestinationId"
                        AND "Candidate"."FundId" = "Duplicate"."FundId");

                DELETE FROM "SpendingTransactionDestinations" AS "Duplicate"
                WHERE "Duplicate"."LocationId" IS NOT NULL
                    AND "Duplicate"."Id" <> (
                        SELECT min("Candidate"."Id")
                        FROM "SpendingTransactionDestinations" AS "Candidate"
                        WHERE "Candidate"."SpendingTransactionId" = "Duplicate"."SpendingTransactionId"
                            AND "Candidate"."LocationId" = "Duplicate"."LocationId");
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountTransaction_SourceLocationId",
                table: "Transactions",
                column: "AccountTransaction_SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IncomeTransaction_SourceLocationId",
                table: "Transactions",
                column: "IncomeTransaction_SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SpendingTransactionDestinations_LocationId",
                table: "SpendingTransactionDestinations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransactionDestinations_LocationId",
                table: "AccountTransactionDestinations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_NormalizedName",
                table: "Locations",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountTransactionDestinations_Locations_LocationId",
                table: "AccountTransactionDestinations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SpendingTransactionDestinations_Locations_LocationId",
                table: "SpendingTransactionDestinations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Locations_AccountTransaction_SourceLocationId",
                table: "Transactions",
                column: "AccountTransaction_SourceLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Locations_IncomeTransaction_SourceLocationId",
                table: "Transactions",
                column: "IncomeTransaction_SourceLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountTransactionDestinations_Locations_LocationId",
                table: "AccountTransactionDestinations");

            migrationBuilder.DropForeignKey(
                name: "FK_SpendingTransactionDestinations_Locations_LocationId",
                table: "SpendingTransactionDestinations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Locations_AccountTransaction_SourceLocationId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Locations_IncomeTransaction_SourceLocationId",
                table: "Transactions");

            migrationBuilder.Sql("""
                UPDATE "Transactions"
                SET "IncomeTransaction_SourceLocationId" = (
                    SELECT "Name" FROM "Locations" WHERE "Id" = "IncomeTransaction_SourceLocationId")
                WHERE "IncomeTransaction_SourceLocationId" IS NOT NULL;
                UPDATE "Transactions"
                SET "AccountTransaction_SourceLocationId" = (
                    SELECT "Name" FROM "Locations" WHERE "Id" = "AccountTransaction_SourceLocationId")
                WHERE "AccountTransaction_SourceLocationId" IS NOT NULL;
                UPDATE "SpendingTransactionDestinations"
                SET "LocationId" = (SELECT "Name" FROM "Locations" WHERE "Id" = "LocationId")
                WHERE "LocationId" IS NOT NULL;
                UPDATE "AccountTransactionDestinations"
                SET "LocationId" = (SELECT "Name" FROM "Locations" WHERE "Id" = "LocationId")
                WHERE "LocationId" IS NOT NULL;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountTransaction_SourceLocationId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_IncomeTransaction_SourceLocationId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_SpendingTransactionDestinations_LocationId",
                table: "SpendingTransactionDestinations");

            migrationBuilder.DropIndex(
                name: "IX_AccountTransactionDestinations_LocationId",
                table: "AccountTransactionDestinations");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.RenameColumn(
                name: "IncomeTransaction_SourceLocationId",
                table: "Transactions",
                newName: "IncomeTransaction_SourceLocation");

            migrationBuilder.RenameColumn(
                name: "AccountTransaction_SourceLocationId",
                table: "Transactions",
                newName: "AccountTransaction_SourceLocation");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "SpendingTransactionDestinations",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "AccountTransactionDestinations",
                newName: "Location");
        }

        private static string NormalizeLegacyLocation(string table, string column) => $$"""
            UPDATE "{{table}}"
            SET "{{column}}" = (
                WITH RECURSIVE "Normalized"("Name") AS (
                    SELECT trim(replace(replace(replace("{{column}}", char(9), ' '), char(10), ' '), char(13), ' '))
                    UNION ALL
                    SELECT replace("Name", '  ', ' ') FROM "Normalized" WHERE instr("Name", '  ') > 0
                )
                SELECT "Id"
                FROM "Locations"
                WHERE "NormalizedName" = (
                    SELECT upper("Name") FROM "Normalized" WHERE instr("Name", '  ') = 0))
            WHERE "{{column}}" IS NOT NULL AND trim("{{column}}") <> '';
            """;
    }
}