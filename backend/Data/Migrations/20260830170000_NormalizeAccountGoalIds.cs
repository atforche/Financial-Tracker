using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations;

/// <summary>
/// Normalizes Account Goal identifiers created by the initial backfill.
/// </summary>
[DbContext(typeof(DatabaseContext))]
[Migration("20260830170000_NormalizeAccountGoalIds")]
public partial class NormalizeAccountGoalIds : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "AccountGoals"
            SET "Id" = upper("Id")
            WHERE "Id" != upper("Id");
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This data repair is intentionally forward-only.
    }
}
