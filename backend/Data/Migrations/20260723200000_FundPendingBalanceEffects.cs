using System;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260723200000_FundPendingBalanceEffects")]
    public partial class FundPendingBalanceEffects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingFundBalanceEffects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PendingDebitAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingCreditAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingFundBalanceEffects", x => x.Id);
                    table.ForeignKey("FK_PendingFundBalanceEffects_Funds_FundId", x => x.FundId, "Funds", "Id", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex("IX_PendingFundBalanceEffects_FundId", "PendingFundBalanceEffects", "FundId");
            migrationBuilder.CreateIndex("IX_PendingFundBalanceEffects_TransactionId_FundId", "PendingFundBalanceEffects", new[] { "TransactionId", "FundId" }, unique: true);

            migrationBuilder.CreateTable(
                name: "PendingFundPlanTotalsEffects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PendingAmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountSpent = table.Column<decimal>(type: "TEXT", nullable: false),
                },
                constraints: table => table.PrimaryKey("PK_PendingFundPlanTotalsEffects", x => x.Id));
            migrationBuilder.CreateIndex("IX_PendingFundPlanTotalsEffects_FundId_AccountingPeriodId", "PendingFundPlanTotalsEffects", new[] { "FundId", "AccountingPeriodId" });
            migrationBuilder.CreateIndex("IX_PendingFundPlanTotalsEffects_TransactionId_FundId_AccountingPeriodId", "PendingFundPlanTotalsEffects", new[] { "TransactionId", "FundId", "AccountingPeriodId" }, unique: true);

            migrationBuilder.DropColumn("PendingCreditAmount", "FundBalanceHistories");
            migrationBuilder.DropColumn("PendingDebitAmount", "FundBalanceHistories");
            migrationBuilder.DropColumn("PendingAmountAssigned", "FundPlanTotalsHistories");
            migrationBuilder.DropColumn("PendingAmountSpent", "FundPlanTotalsHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>("PendingCreditAmount", "FundBalanceHistories", type: "TEXT", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>("PendingDebitAmount", "FundBalanceHistories", type: "TEXT", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>("PendingAmountAssigned", "FundPlanTotalsHistories", type: "TEXT", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>("PendingAmountSpent", "FundPlanTotalsHistories", type: "TEXT", nullable: false, defaultValue: 0m);
            migrationBuilder.DropTable("PendingFundBalanceEffects");
            migrationBuilder.DropTable("PendingFundPlanTotalsEffects");
        }
    }
}
