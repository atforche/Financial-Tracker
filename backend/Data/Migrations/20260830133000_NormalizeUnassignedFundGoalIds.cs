using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations;

/// <summary>
/// Normalizes Fund Goal identifiers created by the initial Unassigned backfill.
/// </summary>
[DbContext(typeof(DatabaseContext))]
[Migration("20260830133000_NormalizeUnassignedFundGoalIds")]
public partial class NormalizeUnassignedFundGoalIds : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "FundGoals"
            SET "Id" = upper("Id")
            WHERE "FundId" = '51A70FF9-49DA-4463-88FD-818B17ACF5C4'
                AND "Id" != upper("Id");
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This data repair is intentionally forward-only.
    }
}
