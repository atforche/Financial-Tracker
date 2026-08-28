using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RefundTransaction_DestinationAccountId",
                table: "Transactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RefundTransaction_DestinationPostedDate",
                table: "Transactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefundTransactionSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    RefundTransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundTransactionSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundTransactionSources_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RefundTransactionSources_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundTransactionSources_Transactions_RefundTransactionId",
                        column: x => x.RefundTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefundTransactionSourceFundAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    SourceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundTransactionSourceFundAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundTransactionSourceFundAssignments_RefundTransactionSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "RefundTransactionSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RefundTransaction_DestinationAccountId",
                table: "Transactions",
                column: "RefundTransaction_DestinationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundTransactionSourceFundAssignments_SourceId",
                table: "RefundTransactionSourceFundAssignments",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundTransactionSources_AccountId",
                table: "RefundTransactionSources",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundTransactionSources_LocationId",
                table: "RefundTransactionSources",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundTransactionSources_RefundTransactionId",
                table: "RefundTransactionSources",
                column: "RefundTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_RefundTransaction_DestinationAccountId",
                table: "Transactions",
                column: "RefundTransaction_DestinationAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_RefundTransaction_DestinationAccountId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "RefundTransactionSourceFundAssignments");

            migrationBuilder.DropTable(
                name: "RefundTransactionSources");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_RefundTransaction_DestinationAccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RefundTransaction_DestinationAccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RefundTransaction_DestinationPostedDate",
                table: "Transactions");
        }
    }
}
