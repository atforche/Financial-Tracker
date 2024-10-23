using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountingPeriods",
                columns: table => new
                {
                    PrimaryKey = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    IsOpen = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriods", x => x.PrimaryKey);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    PrimaryKey = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.PrimaryKey);
                });

            migrationBuilder.CreateTable(
                name: "AccountStartingBalances",
                columns: table => new
                {
                    PrimaryKey = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountStartingBalances", x => x.PrimaryKey);
                });

            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    PrimaryKey = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.PrimaryKey);
                });

            migrationBuilder.CreateTable(
                name: "TransactionDetails",
                columns: table => new
                {
                    PrimaryKey = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StatementDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    IsPosted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionDetails", x => x.PrimaryKey);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    PrimaryKey = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DebitDetailPrimaryKey = table.Column<long>(type: "INTEGER", nullable: true),
                    CreditDetailPrimaryKey = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.PrimaryKey);
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionDetails_CreditDetailPrimaryKey",
                        column: x => x.CreditDetailPrimaryKey,
                        principalTable: "TransactionDetails",
                        principalColumn: "PrimaryKey");
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionDetails_DebitDetailPrimaryKey",
                        column: x => x.DebitDetailPrimaryKey,
                        principalTable: "TransactionDetails",
                        principalColumn: "PrimaryKey");
                });

            migrationBuilder.CreateTable(
                name: "FundAmounts",
                columns: table => new
                {
                    PrimaryKey = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountStartingBalanceDataPrimaryKey = table.Column<long>(type: "INTEGER", nullable: true),
                    TransactionDataPrimaryKey = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundAmounts", x => x.PrimaryKey);
                    table.ForeignKey(
                        name: "FK_FundAmounts_AccountStartingBalances_AccountStartingBalanceDataPrimaryKey",
                        column: x => x.AccountStartingBalanceDataPrimaryKey,
                        principalTable: "AccountStartingBalances",
                        principalColumn: "PrimaryKey");
                    table.ForeignKey(
                        name: "FK_FundAmounts_Transactions_TransactionDataPrimaryKey",
                        column: x => x.TransactionDataPrimaryKey,
                        principalTable: "Transactions",
                        principalColumn: "PrimaryKey");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_Id",
                table: "AccountingPeriods",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_Year_Month",
                table: "AccountingPeriods",
                columns: new[] { "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Id",
                table: "Accounts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountStartingBalances_AccountId",
                table: "AccountStartingBalances",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountStartingBalances_AccountingPeriodId",
                table: "AccountStartingBalances",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAmounts_AccountStartingBalanceDataPrimaryKey",
                table: "FundAmounts",
                column: "AccountStartingBalanceDataPrimaryKey");

            migrationBuilder.CreateIndex(
                name: "IX_FundAmounts_TransactionDataPrimaryKey",
                table: "FundAmounts",
                column: "TransactionDataPrimaryKey");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Id",
                table: "Funds",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreditDetailPrimaryKey",
                table: "Transactions",
                column: "CreditDetailPrimaryKey");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DebitDetailPrimaryKey",
                table: "Transactions",
                column: "DebitDetailPrimaryKey");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Id",
                table: "Transactions",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "FundAmounts");

            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "AccountStartingBalances");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "TransactionDetails");
        }
    }
}