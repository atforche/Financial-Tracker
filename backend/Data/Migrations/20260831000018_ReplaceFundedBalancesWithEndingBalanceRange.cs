using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceFundedBalancesWithEndingBalanceRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinimumFundedBalance",
                table: "FundGoals",
                newName: "MinimumEndingBalance");

            migrationBuilder.RenameColumn(
                name: "MaximumFundedBalance",
                table: "FundGoals",
                newName: "MaximumEndingBalance");

            // A legacy exact ending target is more specific than the former funded
            // bounds. Preserve it as an exact ending-balance range when present.
            migrationBuilder.Sql("""
                UPDATE "FundGoals"
                SET "MinimumEndingBalance" = "TargetEndingBalance",
                    "MaximumEndingBalance" = "TargetEndingBalance"
                WHERE "TargetEndingBalance" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "TargetEndingBalance",
                table: "FundGoals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TargetEndingBalance",
                table: "FundGoals",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "FundGoals"
                SET "TargetEndingBalance" = "MinimumEndingBalance"
                WHERE "MinimumEndingBalance" = "MaximumEndingBalance";
                """);

            migrationBuilder.RenameColumn(
                name: "MinimumEndingBalance",
                table: "FundGoals",
                newName: "MinimumFundedBalance");

            migrationBuilder.RenameColumn(
                name: "MaximumEndingBalance",
                table: "FundGoals",
                newName: "MaximumFundedBalance");
        }
    }
}
