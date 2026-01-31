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
                name: "TransactionAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Account = table.Column<Guid>(type: "TEXT", nullable: false),
                    PostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionAccount_Accounts_Account",
                        column: x => x.Account,
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
                name: "TransactionAccountFundAmounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TransactionAccountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionAccountFundAmounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionAccountFundAmounts_TransactionAccount_TransactionAccountId",
                        column: x => x.TransactionAccountId,
                        principalTable: "TransactionAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriod = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    DebitAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreditAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    InitialAccountTransaction = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionAccount_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalTable: "TransactionAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionAccount_DebitAccountId",
                        column: x => x.DebitAccountId,
                        principalTable: "TransactionAccount",
                        principalColumn: "Id");
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
                name: "IX_TransactionAccount_Account",
                table: "TransactionAccount",
                column: "Account");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionAccountFundAmounts_TransactionAccountId",
                table: "TransactionAccountFundAmounts",
                column: "TransactionAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreditAccountId",
                table: "Transactions",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DebitAccountId",
                table: "Transactions",
                column: "DebitAccountId");
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
                name: "TransactionAccountFundAmounts");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "AccountBalanceHistories");

            migrationBuilder.DropTable(
                name: "FundBalanceHistories");

            migrationBuilder.DropTable(
                name: "TransactionAccount");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
