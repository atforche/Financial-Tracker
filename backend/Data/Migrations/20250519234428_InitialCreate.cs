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
                    Key_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Key_Month = table.Column<int>(type: "INTEGER", nullable: false),
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
                    Name = table.Column<string>(type: "TEXT", nullable: false)
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
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false),
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_AccountingPeriods_AccountingPeriodId",
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
                    AccountAddedBalanceEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountAddedBalanceEventId1 = table.Column<Guid>(type: "TEXT", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_Accounts_AccountAddedBalanceEvent_AccountAddedBalanceEventId1",
                        column: x => x.AccountAddedBalanceEventId1,
                        principalTable: "AccountAddedBalanceEvent",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccountBalanceCheckpoint",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceCheckpoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBalanceCheckpoint_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundConversion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromFundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToFundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundConversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundConversion_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundConversion_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundConversion_Funds_FromFundId",
                        column: x => x.FromFundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundConversion_Funds_ToFundId",
                        column: x => x.ToFundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionBalanceEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionEventType = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionAccountType = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionBalanceEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionBalanceEvent_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionBalanceEvent_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: true)
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
                        name: "FK_FundAmount_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChangeInValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundAmountId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeInValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeInValue_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChangeInValue_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChangeInValue_FundAmount_FundAmountId",
                        column: x => x.FundAmountId,
                        principalTable: "FundAmount",
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
                name: "IX_Accounts_AccountAddedBalanceEventId",
                table: "Accounts",
                column: "AccountAddedBalanceEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountAddedBalanceEventId1",
                table: "Accounts",
                column: "AccountAddedBalanceEventId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangeInValue_AccountId",
                table: "ChangeInValue",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeInValue_AccountingPeriodId",
                table: "ChangeInValue",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeInValue_FundAmountId",
                table: "ChangeInValue",
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
                name: "IX_FundAmount_FundId",
                table: "FundAmount",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAmount_TransactionId",
                table: "FundAmount",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FundConversion_AccountId",
                table: "FundConversion",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FundConversion_AccountingPeriodId",
                table: "FundConversion",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FundConversion_FromFundId",
                table: "FundConversion",
                column: "FromFundId");

            migrationBuilder.CreateIndex(
                name: "IX_FundConversion_ToFundId",
                table: "FundConversion",
                column: "ToFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                table: "Funds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountingPeriodId",
                table: "Transaction",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBalanceEvent_AccountId",
                table: "TransactionBalanceEvent",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBalanceEvent_EventDate_EventSequence",
                table: "TransactionBalanceEvent",
                columns: new[] { "EventDate", "EventSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBalanceEvent_TransactionId",
                table: "TransactionBalanceEvent",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeInValue");

            migrationBuilder.DropTable(
                name: "FundConversion");

            migrationBuilder.DropTable(
                name: "TransactionBalanceEvent");

            migrationBuilder.DropTable(
                name: "FundAmount");

            migrationBuilder.DropTable(
                name: "AccountBalanceCheckpoint");

            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "AccountAddedBalanceEvent");

            migrationBuilder.DropTable(
                name: "AccountingPeriods");
        }
    }
}