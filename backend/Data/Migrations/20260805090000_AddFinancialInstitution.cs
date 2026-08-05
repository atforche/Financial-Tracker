using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations;

/// <summary>
/// Adds the optional FinancialInstitution column to Accounts.
/// </summary>
[DbContext(typeof(DatabaseContext))]
[Migration("20260805090000_AddFinancialInstitution")]
public partial class AddFinancialInstitution : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
        name: "FinancialInstitution",
        table: "Accounts",
        type: "TEXT",
        nullable: true);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
        name: "FinancialInstitution",
        table: "Accounts");
}
