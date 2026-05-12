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
                    OpeningAccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DateOpened = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    OnboardedBalance = table.Column<decimal>(type: "TEXT", nullable: true)
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
                    AmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountSpent = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountSpent = table.Column<decimal>(type: "TEXT", nullable: false)
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
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    OpeningAccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OnboardedBalance = table.Column<decimal>(type: "TEXT", nullable: true)
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
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GoalType = table.Column<string>(type: "TEXT", nullable: false),
                    GoalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    RemainingAmountToAssign = table.Column<decimal>(type: "TEXT", nullable: false),
                    RemainingAmountToAssignIncludingPending = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsAssignmentGoalMet = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAssignmentGoalMetIncludingPending = table.Column<bool>(type: "INTEGER", nullable: false),
                    RemainingAmountToSpend = table.Column<decimal>(type: "TEXT", nullable: false),
                    RemainingAmountToSpendIncludingPending = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsSpendingGoalMet = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSpendingGoalMetIncludingPending = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goals_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
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
                    AccountTransaction_DebitAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AccountTransaction_DebitPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    AccountTransaction_CreditAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AccountTransaction_CreditPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    FundTransaction_DebitFundId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FundTransaction_CreditFundId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IncomeTransaction_CreditAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IncomeTransaction_CreditPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    IncomeTransaction_DebitAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IncomeTransaction_DebitPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    SpendingTransaction_DebitAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SpendingTransaction_DebitPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    SpendingTransaction_CreditAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SpendingTransaction_CreditPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountTransaction_CreditAccountId",
                        column: x => x.AccountTransaction_CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountTransaction_DebitAccountId",
                        column: x => x.AccountTransaction_DebitAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_IncomeTransaction_CreditAccountId",
                        column: x => x.IncomeTransaction_CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_IncomeTransaction_DebitAccountId",
                        column: x => x.IncomeTransaction_DebitAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_SpendingTransaction_CreditAccountId",
                        column: x => x.SpendingTransaction_CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_SpendingTransaction_DebitAccountId",
                        column: x => x.SpendingTransaction_DebitAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Funds_FundTransaction_CreditFundId",
                        column: x => x.FundTransaction_CreditFundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Funds_FundTransaction_DebitFundId",
                        column: x => x.FundTransaction_DebitFundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountingPeriodAccountBalanceHistory",
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
                    table.PrimaryKey("PK_AccountingPeriodAccountBalanceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodAccountBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId",
                        column: x => x.AccountingPeriodBalanceHistoryId,
                        principalTable: "AccountingPeriodBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodAccountBalanceHistory_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodAccountBalanceHistory_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountingPeriodFundBalanceHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountSpent = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountSpent = table.Column<decimal>(type: "TEXT", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountingPeriodBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriodFundBalanceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodFundBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId",
                        column: x => x.AccountingPeriodBalanceHistoryId,
                        principalTable: "AccountingPeriodBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodFundBalanceHistory_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodFundBalanceHistory_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomeTransactionFundAssignments",
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
                    table.PrimaryKey("PK_IncomeTransactionFundAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeTransactionFundAssignments_Transactions_IncomeTransactionId",
                        column: x => x.IncomeTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpendingTransactionFundAssignments",
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
                    table.PrimaryKey("PK_SpendingTransactionFundAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpendingTransactionFundAssignments_Transactions_SpendingTransactionId",
                        column: x => x.SpendingTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceHistories_AccountId",
                table: "AccountBalanceHistories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodAccountBalanceHistory_AccountId",
                table: "AccountingPeriodAccountBalanceHistory",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodAccountBalanceHistory_AccountingPeriodBalanceHistoryId",
                table: "AccountingPeriodAccountBalanceHistory",
                column: "AccountingPeriodBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodAccountBalanceHistory_AccountingPeriodId",
                table: "AccountingPeriodAccountBalanceHistory",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodBalanceHistories_AccountingPeriodId",
                table: "AccountingPeriodBalanceHistories",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodFundBalanceHistory_AccountingPeriodBalanceHistoryId",
                table: "AccountingPeriodFundBalanceHistory",
                column: "AccountingPeriodBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodFundBalanceHistory_AccountingPeriodId",
                table: "AccountingPeriodFundBalanceHistory",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodFundBalanceHistory_FundId",
                table: "AccountingPeriodFundBalanceHistory",
                column: "FundId");

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
                name: "IX_Funds_Name",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_FundId",
                table: "Goals",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTransactionFundAssignments_IncomeTransactionId",
                table: "IncomeTransactionFundAssignments",
                column: "IncomeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_SpendingTransactionFundAssignments_SpendingTransactionId",
                table: "SpendingTransactionFundAssignments",
                column: "SpendingTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountTransaction_CreditAccountId",
                table: "Transactions",
                column: "AccountTransaction_CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountTransaction_DebitAccountId",
                table: "Transactions",
                column: "AccountTransaction_DebitAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FundTransaction_CreditFundId",
                table: "Transactions",
                column: "FundTransaction_CreditFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FundTransaction_DebitFundId",
                table: "Transactions",
                column: "FundTransaction_DebitFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IncomeTransaction_CreditAccountId",
                table: "Transactions",
                column: "IncomeTransaction_CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IncomeTransaction_DebitAccountId",
                table: "Transactions",
                column: "IncomeTransaction_DebitAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SpendingTransaction_CreditAccountId",
                table: "Transactions",
                column: "SpendingTransaction_CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SpendingTransaction_DebitAccountId",
                table: "Transactions",
                column: "SpendingTransaction_DebitAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountBalanceHistories");

            migrationBuilder.DropTable(
                name: "AccountingPeriodAccountBalanceHistory");

            migrationBuilder.DropTable(
                name: "AccountingPeriodFundBalanceHistory");

            migrationBuilder.DropTable(
                name: "FundBalanceHistories");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "IncomeTransactionFundAssignments");

            migrationBuilder.DropTable(
                name: "SpendingTransactionFundAssignments");

            migrationBuilder.DropTable(
                name: "AccountingPeriodBalanceHistories");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Funds");
        }
    }
}
