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
                    InternalId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Key_Month = table.Column<int>(type: "INTEGER", nullable: false),
                    IsOpen = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExternalId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriods", x => x.InternalId);
                });

            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ExternalId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.InternalId);
                });

            migrationBuilder.CreateTable(
                name: "AccountAddedBalanceEvent",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountingPeriodId = table.Column<long>(type: "INTEGER", nullable: true),
                    ExternalId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAddedBalanceEvent", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_AccountAddedBalanceEvent_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "InternalId");
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountingPeriodId = table.Column<long>(type: "INTEGER", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExternalId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_Transaction_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    AccountAddedBalanceEventId = table.Column<long>(type: "INTEGER", nullable: false),
                    AccountAddedBalanceEvent_internalId = table.Column<long>(type: "INTEGER", nullable: true),
                    ExternalId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_Accounts_AccountAddedBalanceEvent_AccountAddedBalanceEventId",
                        column: x => x.AccountAddedBalanceEventId,
                        principalTable: "AccountAddedBalanceEvent",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_AccountAddedBalanceEvent_AccountAddedBalanceEvent_internalId",
                        column: x => x.AccountAddedBalanceEvent_internalId,
                        principalTable: "AccountAddedBalanceEvent",
                        principalColumn: "InternalId");
                });

            migrationBuilder.CreateTable(
                name: "AccountBalanceCheckpoint",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceCheckpoint", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_AccountBalanceCheckpoint_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundConversion",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromFundId = table.Column<long>(type: "INTEGER", nullable: false),
                    ToFundId = table.Column<long>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<long>(type: "INTEGER", nullable: true),
                    ExternalId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundConversion", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_FundConversion_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "InternalId");
                    table.ForeignKey(
                        name: "FK_FundConversion_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundConversion_Funds_FromFundId",
                        column: x => x.FromFundId,
                        principalTable: "Funds",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundConversion_Funds_ToFundId",
                        column: x => x.ToFundId,
                        principalTable: "Funds",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionBalanceEvent",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionId = table.Column<long>(type: "INTEGER", nullable: false),
                    TransactionEventType = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionAccountType = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionBalanceEvent", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_TransactionBalanceEvent_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionBalanceEvent_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundAmount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<long>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountAddedBalanceEventId = table.Column<long>(type: "INTEGER", nullable: true),
                    AccountBalanceCheckpointId = table.Column<long>(type: "INTEGER", nullable: true),
                    TransactionId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundAmount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundAmount_AccountAddedBalanceEvent_AccountAddedBalanceEventId",
                        column: x => x.AccountAddedBalanceEventId,
                        principalTable: "AccountAddedBalanceEvent",
                        principalColumn: "InternalId");
                    table.ForeignKey(
                        name: "FK_FundAmount_AccountBalanceCheckpoint_AccountBalanceCheckpointId",
                        column: x => x.AccountBalanceCheckpointId,
                        principalTable: "AccountBalanceCheckpoint",
                        principalColumn: "InternalId");
                    table.ForeignKey(
                        name: "FK_FundAmount_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "InternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundAmount_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "InternalId");
                });

            migrationBuilder.CreateTable(
                name: "ChangeInValue",
                columns: table => new
                {
                    InternalId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundAmountId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodId = table.Column<long>(type: "INTEGER", nullable: true),
                    ExternalId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountingPeriodKey_Year = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountingPeriodKey_Month = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EventSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeInValue", x => x.InternalId);
                    table.ForeignKey(
                        name: "FK_ChangeInValue_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "InternalId");
                    table.ForeignKey(
                        name: "FK_ChangeInValue_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "InternalId",
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
                name: "IX_AccountAddedBalanceEvent_ExternalId",
                table: "AccountAddedBalanceEvent",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceCheckpoint_AccountId",
                table: "AccountBalanceCheckpoint",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalanceCheckpoint_ExternalId",
                table: "AccountBalanceCheckpoint",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_ExternalId",
                table: "AccountingPeriods",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountAddedBalanceEvent_internalId",
                table: "Accounts",
                column: "AccountAddedBalanceEvent_internalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountAddedBalanceEventId",
                table: "Accounts",
                column: "AccountAddedBalanceEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ExternalId",
                table: "Accounts",
                column: "ExternalId",
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
                name: "IX_ChangeInValue_ExternalId",
                table: "ChangeInValue",
                column: "ExternalId",
                unique: true);

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
                name: "IX_FundConversion_ExternalId",
                table: "FundConversion",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundConversion_FromFundId",
                table: "FundConversion",
                column: "FromFundId");

            migrationBuilder.CreateIndex(
                name: "IX_FundConversion_ToFundId",
                table: "FundConversion",
                column: "ToFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_ExternalId",
                table: "Funds",
                column: "ExternalId",
                unique: true);

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
                name: "IX_Transaction_ExternalId",
                table: "Transaction",
                column: "ExternalId",
                unique: true);

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
                name: "IX_TransactionBalanceEvent_ExternalId",
                table: "TransactionBalanceEvent",
                column: "ExternalId",
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