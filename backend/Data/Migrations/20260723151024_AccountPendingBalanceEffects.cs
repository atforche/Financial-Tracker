using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AccountPendingBalanceEffects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingAccountBalanceEffects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PendingDebitAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingCreditAmount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingAccountBalanceEffects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingAccountBalanceEffects_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingAccountBalanceEffects_AccountId",
                table: "PendingAccountBalanceEffects",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingAccountBalanceEffects_TransactionId_AccountId",
                table: "PendingAccountBalanceEffects",
                columns: new[] { "TransactionId", "AccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingAccountBalanceEffects");
        }
    }
}
