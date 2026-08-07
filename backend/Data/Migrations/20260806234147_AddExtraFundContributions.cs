using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraFundContributions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PendingRegularAmountAssigned",
                table: "PendingFundGoalTotalsEffects",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsExtraContribution",
                table: "IncomeTransactionIncomeDestinationFundAssignments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RegularAmountAssigned",
                table: "FundGoalTotalsHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RegularAmountAssigned",
                table: "AccountingPeriodFundGoalTotals",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE "PendingFundGoalTotalsEffects"
                SET "PendingRegularAmountAssigned" = "PendingAmountAssigned"
                """);
            migrationBuilder.Sql("""
                UPDATE "FundGoalTotalsHistories"
                SET "RegularAmountAssigned" = "AmountAssigned"
                """);
            migrationBuilder.Sql("""
                UPDATE "AccountingPeriodFundGoalTotals"
                SET "RegularAmountAssigned" = "AmountAssigned"
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingRegularAmountAssigned",
                table: "PendingFundGoalTotalsEffects");

            migrationBuilder.DropColumn(
                name: "IsExtraContribution",
                table: "IncomeTransactionIncomeDestinationFundAssignments");

            migrationBuilder.DropColumn(
                name: "RegularAmountAssigned",
                table: "FundGoalTotalsHistories");

            migrationBuilder.DropColumn(
                name: "RegularAmountAssigned",
                table: "AccountingPeriodFundGoalTotals");
        }
    }
}
