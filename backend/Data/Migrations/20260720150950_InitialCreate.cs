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
                    PendingDebitAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingCreditAmount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundBalanceHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundPlanTotalsHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    AmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountSpent = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountSpent = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundPlanTotalsHistories", x => x.Id);
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
                    PendingCreditAmount = table.Column<decimal>(type: "TEXT", nullable: false)
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
                name: "FundPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RegularContribution = table.Column<decimal>(type: "TEXT", nullable: true),
                    MinimumFundedBalance = table.Column<decimal>(type: "TEXT", nullable: true),
                    MaximumFundedBalance = table.Column<decimal>(type: "TEXT", nullable: true),
                    TargetEndingBalance = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundPlans_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundPlans_Funds_FundId",
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
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountTransaction_DebitAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AccountTransaction_DebitPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    AccountTransaction_SourceLocation = table.Column<string>(type: "TEXT", nullable: true),
                    FundTransaction_DebitFundId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IncomeTransaction_SourceAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IncomeTransaction_SourcePostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    IncomeTransaction_SourceLocation = table.Column<string>(type: "TEXT", nullable: true),
                    TrackedAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    SpendingTransaction_DebitAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SpendingTransaction_DebitPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountTransaction_DebitAccountId",
                        column: x => x.AccountTransaction_DebitAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_IncomeTransaction_SourceAccountId",
                        column: x => x.IncomeTransaction_SourceAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_SpendingTransaction_DebitAccountId",
                        column: x => x.SpendingTransaction_DebitAccountId,
                        principalTable: "Accounts",
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
                name: "AccountingPeriodFundPlanTotals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountAssigned = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountSpent = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingAmountSpent = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountingPeriodBalanceHistoryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriodFundPlanTotals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodFundPlanTotals_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId",
                        column: x => x.AccountingPeriodBalanceHistoryId,
                        principalTable: "AccountingPeriodBalanceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodFundPlanTotals_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodFundPlanTotals_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountTransactionDestinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreditAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreditPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountTransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTransactionDestinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountTransactionDestinations_Accounts_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccountTransactionDestinations_Transactions_AccountTransactionId",
                        column: x => x.AccountTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundTransactionDestinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreditFundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    FundTransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTransactionDestinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundTransactionDestinations_Funds_CreditFundId",
                        column: x => x.CreditFundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundTransactionDestinations_Transactions_FundTransactionId",
                        column: x => x.FundTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomeTransactionIncomeDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeTransactionId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeTransactionIncomeDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeTransactionIncomeDeductions_Transactions_IncomeTransactionId",
                        column: x => x.IncomeTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomeTransactionIncomeDestinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeTransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeTransactionIncomeDestinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeTransactionIncomeDestinations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncomeTransactionIncomeDestinations_Transactions_IncomeTransactionId",
                        column: x => x.IncomeTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomeTransactionIncomeLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeTransactionId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeTransactionIncomeLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeTransactionIncomeLines_Transactions_IncomeTransactionId",
                        column: x => x.IncomeTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpendingTransactionDestinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreditAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreditPostedDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    SpendingTransactionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpendingTransactionDestinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpendingTransactionDestinations_Accounts_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SpendingTransactionDestinations_Transactions_SpendingTransactionId",
                        column: x => x.SpendingTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomeTransactionIncomeDestinationFundAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeDestinationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeTransactionIncomeDestinationFundAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeTransactionIncomeDestinationFundAssignments_IncomeTransactionIncomeDestinations_IncomeDestinationId",
                        column: x => x.IncomeDestinationId,
                        principalTable: "IncomeTransactionIncomeDestinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpendingTransactionDestinationFundAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    DestinationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpendingTransactionDestinationFundAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpendingTransactionDestinationFundAssignments_SpendingTransactionDestinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "SpendingTransactionDestinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceHistories_AccountId_Date_Sequence",
                table: "AccountBalanceHistories",
                columns: new[] { "AccountId", "Date", "Sequence" },
                unique: true);

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
                name: "IX_AccountingPeriodFundPlanTotals_AccountingPeriodBalanceHistoryId",
                table: "AccountingPeriodFundPlanTotals",
                column: "AccountingPeriodBalanceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodFundPlanTotals_AccountingPeriodId",
                table: "AccountingPeriodFundPlanTotals",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodFundPlanTotals_FundId",
                table: "AccountingPeriodFundPlanTotals",
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
                name: "IX_AccountTransactionDestinations_AccountTransactionId",
                table: "AccountTransactionDestinations",
                column: "AccountTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransactionDestinations_CreditAccountId",
                table: "AccountTransactionDestinations",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FundPlans_AccountingPeriodId",
                table: "FundPlans",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FundPlans_FundId",
                table: "FundPlans",
                column: "FundId",
                unique: true,
                filter: "\"AccountingPeriodId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FundPlans_FundId_AccountingPeriodId",
                table: "FundPlans",
                columns: new[] { "FundId", "AccountingPeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundPlanTotalsHistories_FundId_AccountingPeriodId_Date_Sequence",
                table: "FundPlanTotalsHistories",
                columns: new[] { "FundId", "AccountingPeriodId", "Date", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactionDestinations_CreditFundId",
                table: "FundTransactionDestinations",
                column: "CreditFundId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactionDestinations_FundTransactionId",
                table: "FundTransactionDestinations",
                column: "FundTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTransactionIncomeDeductions_IncomeTransactionId",
                table: "IncomeTransactionIncomeDeductions",
                column: "IncomeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTransactionIncomeDestinationFundAssignments_IncomeDestinationId",
                table: "IncomeTransactionIncomeDestinationFundAssignments",
                column: "IncomeDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTransactionIncomeDestinations_AccountId",
                table: "IncomeTransactionIncomeDestinations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTransactionIncomeDestinations_IncomeTransactionId",
                table: "IncomeTransactionIncomeDestinations",
                column: "IncomeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTransactionIncomeLines_IncomeTransactionId",
                table: "IncomeTransactionIncomeLines",
                column: "IncomeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_SpendingTransactionDestinationFundAssignments_DestinationId",
                table: "SpendingTransactionDestinationFundAssignments",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_SpendingTransactionDestinations_CreditAccountId",
                table: "SpendingTransactionDestinations",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SpendingTransactionDestinations_SpendingTransactionId",
                table: "SpendingTransactionDestinations",
                column: "SpendingTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountTransaction_DebitAccountId",
                table: "Transactions",
                column: "AccountTransaction_DebitAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Date_Sequence",
                table: "Transactions",
                columns: new[] { "Date", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FundTransaction_DebitFundId",
                table: "Transactions",
                column: "FundTransaction_DebitFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IncomeTransaction_SourceAccountId",
                table: "Transactions",
                column: "IncomeTransaction_SourceAccountId");

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
                name: "AccountingPeriodFundPlanTotals");

            migrationBuilder.DropTable(
                name: "AccountTransactionDestinations");

            migrationBuilder.DropTable(
                name: "FundBalanceHistories");

            migrationBuilder.DropTable(
                name: "FundPlans");

            migrationBuilder.DropTable(
                name: "FundPlanTotalsHistories");

            migrationBuilder.DropTable(
                name: "FundTransactionDestinations");

            migrationBuilder.DropTable(
                name: "IncomeTransactionIncomeDeductions");

            migrationBuilder.DropTable(
                name: "IncomeTransactionIncomeDestinationFundAssignments");

            migrationBuilder.DropTable(
                name: "IncomeTransactionIncomeLines");

            migrationBuilder.DropTable(
                name: "SpendingTransactionDestinationFundAssignments");

            migrationBuilder.DropTable(
                name: "AccountingPeriodBalanceHistories");

            migrationBuilder.DropTable(
                name: "IncomeTransactionIncomeDestinations");

            migrationBuilder.DropTable(
                name: "SpendingTransactionDestinations");

            migrationBuilder.DropTable(
                name: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Funds");
        }
    }
}
