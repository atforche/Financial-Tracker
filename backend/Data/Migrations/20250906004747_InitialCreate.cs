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
                    IsOpen = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriods", x => x.Id);
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
                name: "AccountAddedBalanceEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAddedBalanceEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountAddedBalanceEvent_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    AccountAddedBalanceEventId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_AccountAddedBalanceEvent_AccountAddedBalanceEventId",
                        column: x => x.AccountAddedBalanceEventId,
                        principalTable: "AccountAddedBalanceEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountBalanceCheckpoint",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceCheckpoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBalanceCheckpoint_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountBalanceCheckpoint_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundConversions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromFundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToFundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundConversions_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundConversions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundConversions_Funds_FromFundId",
                        column: x => x.FromFundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundConversions_Funds_ToFundId",
                        column: x => x.ToFundId,
                        principalTable: "Funds",
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
                    DebitAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreditAccountId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_DebitAccountId",
                        column: x => x.DebitAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FundAmount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountAddedBalanceEventId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AccountBalanceCheckpointId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreditTransactionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DebitTransactionId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundAmount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundAmount_AccountAddedBalanceEvent_AccountAddedBalanceEventId",
                        column: x => x.AccountAddedBalanceEventId,
                        principalTable: "AccountAddedBalanceEvent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundAmount_AccountBalanceCheckpoint_AccountBalanceCheckpointId",
                        column: x => x.AccountBalanceCheckpointId,
                        principalTable: "AccountBalanceCheckpoint",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundAmount_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundAmount_Transactions_CreditTransactionId",
                        column: x => x.CreditTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundAmount_Transactions_DebitTransactionId",
                        column: x => x.DebitTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransactionBalanceEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionBalanceEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionBalanceEvent_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeInValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundAmountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeInValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeInValues_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChangeInValues_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChangeInValues_FundAmount_FundAmountId",
                        column: x => x.FundAmountId,
                        principalTable: "FundAmount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionBalanceEventPart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionBalanceEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionBalanceEventPart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionBalanceEventPart_TransactionBalanceEvent_TransactionBalanceEventId",
                        column: x => x.TransactionBalanceEventId,
                        principalTable: "TransactionBalanceEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAddedBalanceEvent_AccountingPeriodId",
                table: "AccountAddedBalanceEvent",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceCheckpoint_AccountId",
                table: "AccountBalanceCheckpoint",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceCheckpoint_AccountingPeriodId",
                table: "AccountBalanceCheckpoint",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountAddedBalanceEventId",
                table: "Accounts",
                column: "AccountAddedBalanceEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangeInValues_AccountId",
                table: "ChangeInValues",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeInValues_AccountingPeriodId",
                table: "ChangeInValues",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeInValues_EventDate_EventSequence",
                table: "ChangeInValues",
                columns: new[] { "EventDate", "EventSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangeInValues_FundAmountId",
                table: "ChangeInValues",
                column: "FundAmountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundAmount_AccountAddedBalanceEventId",
                table: "FundAmount",
                column: "AccountAddedBalanceEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAmount_AccountBalanceCheckpointId",
                table: "FundAmount",
                column: "AccountBalanceCheckpointId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAmount_CreditTransactionId",
                table: "FundAmount",
                column: "CreditTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAmount_DebitTransactionId",
                table: "FundAmount",
                column: "DebitTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAmount_FundId",
                table: "FundAmount",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_FundConversions_AccountId",
                table: "FundConversions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FundConversions_AccountingPeriodId",
                table: "FundConversions",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FundConversions_EventDate_EventSequence",
                table: "FundConversions",
                columns: new[] { "EventDate", "EventSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundConversions_FromFundId",
                table: "FundConversions",
                column: "FromFundId");

            migrationBuilder.CreateIndex(
                name: "IX_FundConversions_ToFundId",
                table: "FundConversions",
                column: "ToFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBalanceEvent_EventDate_EventSequence",
                table: "TransactionBalanceEvent",
                columns: new[] { "EventDate", "EventSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBalanceEvent_TransactionId",
                table: "TransactionBalanceEvent",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBalanceEventPart_TransactionBalanceEventId",
                table: "TransactionBalanceEventPart",
                column: "TransactionBalanceEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountingPeriodId",
                table: "Transactions",
                column: "AccountingPeriodId");

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
                name: "ChangeInValues");

            migrationBuilder.DropTable(
                name: "FundConversions");

            migrationBuilder.DropTable(
                name: "TransactionBalanceEventPart");

            migrationBuilder.DropTable(
                name: "FundAmount");

            migrationBuilder.DropTable(
                name: "TransactionBalanceEvent");

            migrationBuilder.DropTable(
                name: "AccountBalanceCheckpoint");

            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "AccountAddedBalanceEvent");

            migrationBuilder.DropTable(
                name: "AccountingPeriods");
        }
    }
}
