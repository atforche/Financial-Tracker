using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePendingFundGoalTotalsEffects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingFundGoalTotalsEffects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingFundGoalTotalsEffects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PendingAmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountAssignedToExpectedContribution = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountSpent = table.Column<decimal>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingFundGoalTotalsEffects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingFundGoalTotalsEffects_FundId_AccountingPeriodId",
                table: "PendingFundGoalTotalsEffects",
                columns: new[] { "FundId", "AccountingPeriodId" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingFundGoalTotalsEffects_TransactionId_FundId_AccountingPeriodId",
                table: "PendingFundGoalTotalsEffects",
                columns: new[] { "TransactionId", "FundId", "AccountingPeriodId" },
                unique: true);
        }
    }
}
