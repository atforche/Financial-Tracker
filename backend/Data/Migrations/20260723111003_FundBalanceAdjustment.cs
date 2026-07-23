using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class FundBalanceAdjustment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FundBalanceHistories_FundId_Date_Sequence",
                table: "FundBalanceHistories",
                columns: new[] { "FundId", "Date", "Sequence" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FundBalanceHistories_Funds_FundId",
                table: "FundBalanceHistories",
                column: "FundId",
                principalTable: "Funds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundBalanceHistories_Funds_FundId",
                table: "FundBalanceHistories");

            migrationBuilder.DropIndex(
                name: "IX_FundBalanceHistories_FundId_Date_Sequence",
                table: "FundBalanceHistories");
        }
    }
}
