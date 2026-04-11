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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
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
                    AddAccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
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
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    PostedBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingDebitAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingCreditAmount = table.Column<decimal>(type: "TEXT", nullable: false)
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
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AddAccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountingPeriodBalanceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriodBalanceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodBalanceHistories_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountBalanceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    PostedBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingDebitAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingCreditAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AvailableToSpend = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBalanceHistories_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeTransaction_AccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IncomeTransaction_PostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    GeneratedByAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DebitAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DebitPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreditAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreditPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    TransferTransaction_DebitAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TransferTransaction_DebitPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    TransferTransaction_CreditAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TransferTransaction_CreditPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_DebitAccountId",
                        column: x => x.DebitAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_IncomeTransaction_AccountId",
                        column: x => x.IncomeTransaction_AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_TransferTransaction_CreditAccountId",
                        column: x => x.TransferTransaction_CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_TransferTransaction_DebitAccountId",
                        column: x => x.TransferTransaction_DebitAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GoalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsGoalMet = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundGoals_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountAccountingPeriodBalanceHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountingPeriodBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAccountingPeriodBalanceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId",
                        column: x => x.AccountingPeriodBalanceHistoryId,
                        principalTable: "AccountingPeriodBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountAccountingPeriodBalanceHistory_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountAccountingPeriodBalanceHistory_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundAccountingPeriodBalanceHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountSpent = table.Column<decimal>(type: "TEXT", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountingPeriodBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundAccountingPeriodBalanceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId",
                        column: x => x.AccountingPeriodBalanceHistoryId,
                        principalTable: "AccountingPeriodBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundAccountingPeriodBalanceHistory_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundAccountingPeriodBalanceHistory_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomeTransactionFundAmounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeTransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeTransactionFundAmounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeTransactionFundAmounts_Transactions_IncomeTransactionId",
                        column: x => x.IncomeTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpendingTransactionFundAmounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    SpendingTransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpendingTransactionFundAmounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpendingTransactionFundAmounts_Transactions_SpendingTransactionId",
                        column: x => x.SpendingTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAccountingPeriodBalanceHistory_AccountId",
                table: "AccountAccountingPeriodBalanceHistory",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistoryId",
                table: "AccountAccountingPeriodBalanceHistory",
                column: "AccountingPeriodBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountAccountingPeriodBalanceHistory_AccountingPeriodId",
                table: "AccountAccountingPeriodBalanceHistory",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceHistories_AccountId",
                table: "AccountBalanceHistories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodBalanceHistories_AccountingPeriodId",
                table: "AccountingPeriodBalanceHistories",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_Name",
                table: "AccountingPeriods",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistoryId",
                table: "FundAccountingPeriodBalanceHistory",
                column: "AccountingPeriodBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAccountingPeriodBalanceHistory_AccountingPeriodId",
                table: "FundAccountingPeriodBalanceHistory",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAccountingPeriodBalanceHistory_FundId",
                table: "FundAccountingPeriodBalanceHistory",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_FundGoals_FundId",
                table: "FundGoals",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTransactionFundAmounts_IncomeTransactionId",
                table: "IncomeTransactionFundAmounts",
                column: "IncomeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_SpendingTransactionFundAmounts_SpendingTransactionId",
                table: "SpendingTransactionFundAmounts",
                column: "SpendingTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreditAccountId",
                table: "Transactions",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DebitAccountId",
                table: "Transactions",
                column: "DebitAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IncomeTransaction_AccountId",
                table: "Transactions",
                column: "IncomeTransaction_AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransferTransaction_CreditAccountId",
                table: "Transactions",
                column: "TransferTransaction_CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransferTransaction_DebitAccountId",
                table: "Transactions",
                column: "TransferTransaction_DebitAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountAccountingPeriodBalanceHistory");

            migrationBuilder.DropTable(
                name: "AccountBalanceHistories");

            migrationBuilder.DropTable(
                name: "FundAccountingPeriodBalanceHistory");

            migrationBuilder.DropTable(
                name: "FundBalanceHistories");

            migrationBuilder.DropTable(
                name: "FundGoals");

            migrationBuilder.DropTable(
                name: "IncomeTransactionFundAmounts");

            migrationBuilder.DropTable(
                name: "SpendingTransactionFundAmounts");

            migrationBuilder.DropTable(
                name: "AccountingPeriodBalanceHistories");

            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
