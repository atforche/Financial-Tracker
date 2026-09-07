using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(DatabaseContext))]
[Migration("20260907000000_AddFundGoalContributionMaximumOverride")]
public partial class AddFundGoalContributionMaximumOverride : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "AllowExpectedContributionAboveMaximum",
            table: "FundGoals",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AllowExpectedContributionAboveMaximum",
            table: "FundGoals");
    }
}
