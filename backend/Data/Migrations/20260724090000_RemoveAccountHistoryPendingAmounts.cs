using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations;

/// <inheritdoc />
public partial class RemoveAccountHistoryPendingAmounts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("PendingCreditAmount", "AccountBalanceHistories");
        migrationBuilder.DropColumn("PendingDebitAmount", "AccountBalanceHistories");
        migrationBuilder.DropColumn("PendingAmountAssigned", "AccountingPeriodFundPlanTotals");
        migrationBuilder.DropColumn("PendingAmountSpent", "AccountingPeriodFundPlanTotals");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>("PendingCreditAmount", "AccountBalanceHistories", type: "TEXT", nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<decimal>("PendingDebitAmount", "AccountBalanceHistories", type: "TEXT", nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<decimal>("PendingAmountAssigned", "AccountingPeriodFundPlanTotals", type: "TEXT", nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<decimal>("PendingAmountSpent", "AccountingPeriodFundPlanTotals", type: "TEXT", nullable: false, defaultValue: 0m);
    }
}
