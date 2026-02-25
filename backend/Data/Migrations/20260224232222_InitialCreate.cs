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
                name: "AccountBalanceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountingPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    IsOpen = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    InitialAccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InitialDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    InitialTransaction = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundBalanceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundBalanceHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountBalanceHistoryFundBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceHistoryFundBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBalanceHistoryFundBalances_AccountBalanceHistories_AccountBalanceHistoryId",
                        column: x => x.AccountBalanceHistoryId,
                        principalTable: "AccountBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountBalanceHistoryPendingCredits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceHistoryPendingCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBalanceHistoryPendingCredits_AccountBalanceHistories_AccountBalanceHistoryId",
                        column: x => x.AccountBalanceHistoryId,
                        principalTable: "AccountBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountBalanceHistoryPendingDebits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceHistoryPendingDebits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBalanceHistoryPendingDebits_AccountBalanceHistories_AccountBalanceHistoryId",
                        column: x => x.AccountBalanceHistoryId,
                        principalTable: "AccountBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    DebitAccount_AccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DebitAccount_PostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreditAccount_AccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreditAccount_PostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    GeneratedByAccountId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_CreditAccount_AccountId",
                        column: x => x.CreditAccount_AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_DebitAccount_AccountId",
                        column: x => x.DebitAccount_AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundBalanceHistoryAccountBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    FundBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundBalanceHistoryAccountBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundBalanceHistoryAccountBalances_FundBalanceHistories_FundBalanceHistoryId",
                        column: x => x.FundBalanceHistoryId,
                        principalTable: "FundBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundBalanceHistoryPendingCredits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    FundBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundBalanceHistoryPendingCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundBalanceHistoryPendingCredits_FundBalanceHistories_FundBalanceHistoryId",
                        column: x => x.FundBalanceHistoryId,
                        principalTable: "FundBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundBalanceHistoryPendingDebits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    FundBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundBalanceHistoryPendingDebits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundBalanceHistoryPendingDebits_FundBalanceHistories_FundBalanceHistoryId",
                        column: x => x.FundBalanceHistoryId,
                        principalTable: "FundBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCreditAccountFundAmounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TransactionAccountTransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCreditAccountFundAmounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionCreditAccountFundAmounts_Transactions_TransactionAccountTransactionId",
                        column: x => x.TransactionAccountTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionDebitAccountFundAmounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TransactionAccountTransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionDebitAccountFundAmounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionDebitAccountFundAmounts_Transactions_TransactionAccountTransactionId",
                        column: x => x.TransactionAccountTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceHistoryFundBalances_AccountBalanceHistoryId",
                table: "AccountBalanceHistoryFundBalances",
                column: "AccountBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceHistoryPendingCredits_AccountBalanceHistoryId",
                table: "AccountBalanceHistoryPendingCredits",
                column: "AccountBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceHistoryPendingDebits_AccountBalanceHistoryId",
                table: "AccountBalanceHistoryPendingDebits",
                column: "AccountBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundBalanceHistoryAccountBalances_FundBalanceHistoryId",
                table: "FundBalanceHistoryAccountBalances",
                column: "FundBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FundBalanceHistoryPendingCredits_FundBalanceHistoryId",
                table: "FundBalanceHistoryPendingCredits",
                column: "FundBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FundBalanceHistoryPendingDebits_FundBalanceHistoryId",
                table: "FundBalanceHistoryPendingDebits",
                column: "FundBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCreditAccountFundAmounts_TransactionAccountTransactionId",
                table: "TransactionCreditAccountFundAmounts",
                column: "TransactionAccountTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionDebitAccountFundAmounts_TransactionAccountTransactionId",
                table: "TransactionDebitAccountFundAmounts",
                column: "TransactionAccountTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreditAccount_AccountId",
                table: "Transactions",
                column: "CreditAccount_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DebitAccount_AccountId",
                table: "Transactions",
                column: "DebitAccount_AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountBalanceHistoryFundBalances");

            migrationBuilder.DropTable(
                name: "AccountBalanceHistoryPendingCredits");

            migrationBuilder.DropTable(
                name: "AccountBalanceHistoryPendingDebits");

            migrationBuilder.DropTable(
                name: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "FundBalanceHistoryAccountBalances");

            migrationBuilder.DropTable(
                name: "FundBalanceHistoryPendingCredits");

            migrationBuilder.DropTable(
                name: "FundBalanceHistoryPendingDebits");

            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "TransactionCreditAccountFundAmounts");

            migrationBuilder.DropTable(
                name: "TransactionDebitAccountFundAmounts");

            migrationBuilder.DropTable(
                name: "AccountBalanceHistories");

            migrationBuilder.DropTable(
                name: "FundBalanceHistories");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
