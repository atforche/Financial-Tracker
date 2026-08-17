using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeMigratedLocationIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            NormalizeLocationIds(migrationBuilder, "upper");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            NormalizeLocationIds(migrationBuilder, "lower");
        }

        private static void NormalizeLocationIds(MigrationBuilder migrationBuilder, string function) =>
            migrationBuilder.Sql($$"""
                PRAGMA defer_foreign_keys = ON;

                UPDATE "Transactions"
                SET "IncomeTransaction_SourceLocationId" = {{function}}("IncomeTransaction_SourceLocationId")
                WHERE "IncomeTransaction_SourceLocationId" IS NOT NULL;

                UPDATE "Transactions"
                SET "AccountTransaction_SourceLocationId" = {{function}}("AccountTransaction_SourceLocationId")
                WHERE "AccountTransaction_SourceLocationId" IS NOT NULL;

                UPDATE "SpendingTransactionDestinations"
                SET "LocationId" = {{function}}("LocationId")
                WHERE "LocationId" IS NOT NULL;

                UPDATE "AccountTransactionDestinations"
                SET "LocationId" = {{function}}("LocationId")
                WHERE "LocationId" IS NOT NULL;

                UPDATE "Locations"
                SET "Id" = {{function}}("Id");
                """);
    }
}