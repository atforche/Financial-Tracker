using Microsoft.EntityFrameworkCore.Migrations;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Data.Migrations
{
    /// <summary>
    /// Recalculates regular Fund Goal contributions without counting Fund transactions.
    /// </summary>
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260814130000_RepairFundGoalRegularContributionTotals")]
    public partial class RepairFundGoalRegularContributionTotals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "FundGoalTotalsHistories" AS history
                SET "RegularAmountAssigned" = COALESCE((
                    SELECT SUM(assignments."Amount")
                    FROM "Transactions" AS txn
                    INNER JOIN "IncomeTransactionIncomeDestinations" AS destination
                        ON destination."IncomeTransactionId" = txn."Id"
                    INNER JOIN "IncomeTransactionIncomeDestinationFundAssignments" AS assignments
                        ON assignments."IncomeDestinationId" = destination."Id"
                    WHERE txn."AccountingPeriodId" = history."AccountingPeriodId"
                        AND assignments."FundId" = history."FundId"
                        AND destination."PostedDate" IS NOT NULL
                        AND assignments."IsExtraContribution" = 0
                        AND (
                            txn."Date" < history."Date"
                            OR (
                                txn."Date" = history."Date"
                                AND txn."Sequence" <= history."Sequence"
                            )
                        )
                ), 0);
                """);

            migrationBuilder.Sql("""
                UPDATE "AccountingPeriodFundGoalTotals" AS totals
                SET "RegularAmountAssigned" = COALESCE((
                    SELECT SUM(assignments."Amount")
                    FROM "Transactions" AS txn
                    INNER JOIN "IncomeTransactionIncomeDestinations" AS destination
                        ON destination."IncomeTransactionId" = txn."Id"
                    INNER JOIN "IncomeTransactionIncomeDestinationFundAssignments" AS assignments
                        ON assignments."IncomeDestinationId" = destination."Id"
                    WHERE txn."AccountingPeriodId" = totals."AccountingPeriodId"
                        AND assignments."FundId" = totals."FundId"
                        AND destination."PostedDate" IS NOT NULL
                        AND assignments."IsExtraContribution" = 0
                ), 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This data repair is intentionally forward-only.
        }
    }
}
