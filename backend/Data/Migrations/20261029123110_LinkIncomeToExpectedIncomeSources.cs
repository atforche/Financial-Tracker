using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkIncomeToExpectedIncomeSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IncomeTransaction_ExpectedIncomeSourceId",
                table: "Transactions",
                type: "TEXT",
                nullable: true);

            // Income now counts toward Fund Goals only when it is planned as an Expected Income Source. Link existing
            // income to the one source in its Accounting Period whose name its location starts with, leaving income
            // that was marked as an extra contribution unplanned. Stored Fund Goal totals are left as they were posted.
            migrationBuilder.Sql("""
                UPDATE Transactions
                SET IncomeTransaction_ExpectedIncomeSourceId = (
                    SELECT source.Id
                    FROM ExpectedIncomeSources source
                    JOIN Locations location ON location.Id = Transactions.IncomeTransaction_SourceLocationId
                    WHERE source.AccountingPeriodId = Transactions.AccountingPeriodId
                        AND lower(location.Name) LIKE lower(source.Name) || '%')
                WHERE Type = 1
                    AND (
                        SELECT COUNT(*)
                        FROM ExpectedIncomeSources source
                        JOIN Locations location ON location.Id = Transactions.IncomeTransaction_SourceLocationId
                        WHERE source.AccountingPeriodId = Transactions.AccountingPeriodId
                            AND lower(location.Name) LIKE lower(source.Name) || '%') = 1
                    AND NOT EXISTS (
                        SELECT 1
                        FROM IncomeTransactionIncomeDestinations destination
                        JOIN IncomeTransactionIncomeDestinationFundAssignments assignment
                            ON assignment.IncomeDestinationId = destination.Id
                        WHERE destination.IncomeTransactionId = Transactions.Id
                            AND assignment.IsExtraContribution = 1);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncomeTransaction_ExpectedIncomeSourceId",
                table: "Transactions");
        }
    }
}
