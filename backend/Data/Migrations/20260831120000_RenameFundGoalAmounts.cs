using Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260831120000_RenameFundGoalAmounts")]
    public partial class RenameFundGoalAmounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegularContribution",
                table: "FundGoals",
                newName: "PlannedMonthlyContribution");

            migrationBuilder.RenameColumn(
                name: "RegularAmountAssigned",
                table: "FundGoalTotalsHistories",
                newName: "AmountAssignedToExpectedContribution");

            migrationBuilder.RenameColumn(
                name: "PendingRegularAmountAssigned",
                table: "PendingFundGoalTotalsEffects",
                newName: "PendingAmountAssignedToExpectedContribution");

            migrationBuilder.RenameColumn(
                name: "RegularAmountAssigned",
                table: "AccountingPeriodFundGoalTotals",
                newName: "AmountAssignedToExpectedContribution");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlannedMonthlyContribution",
                table: "FundGoals",
                newName: "RegularContribution");

            migrationBuilder.RenameColumn(
                name: "AmountAssignedToExpectedContribution",
                table: "FundGoalTotalsHistories",
                newName: "RegularAmountAssigned");

            migrationBuilder.RenameColumn(
                name: "PendingAmountAssignedToExpectedContribution",
                table: "PendingFundGoalTotalsEffects",
                newName: "PendingRegularAmountAssigned");

            migrationBuilder.RenameColumn(
                name: "AmountAssignedToExpectedContribution",
                table: "AccountingPeriodFundGoalTotals",
                newName: "RegularAmountAssigned");
        }
    }
}
