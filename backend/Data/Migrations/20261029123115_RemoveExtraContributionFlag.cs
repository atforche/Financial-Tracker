using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExtraContributionFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExtraContribution",
                table: "IncomeTransactionIncomeDestinationFundAssignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExtraContribution",
                table: "IncomeTransactionIncomeDestinationFundAssignments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
