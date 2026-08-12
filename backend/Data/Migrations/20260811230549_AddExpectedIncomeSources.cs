using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpectedIncomeSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpectedIncomeSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpectedIncomeSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpectedIncomeSources_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpectedIncomeSourceDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExpectedIncomeSourceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpectedIncomeSourceDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpectedIncomeSourceDates_ExpectedIncomeSources_ExpectedIncomeSourceId",
                        column: x => x.ExpectedIncomeSourceId,
                        principalTable: "ExpectedIncomeSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpectedIncomeSourceIncomeDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExpectedIncomeSourceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpectedIncomeSourceIncomeDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpectedIncomeSourceIncomeDeductions_ExpectedIncomeSources_ExpectedIncomeSourceId",
                        column: x => x.ExpectedIncomeSourceId,
                        principalTable: "ExpectedIncomeSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpectedIncomeSourceIncomeLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExpectedIncomeSourceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpectedIncomeSourceIncomeLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpectedIncomeSourceIncomeLines_ExpectedIncomeSources_ExpectedIncomeSourceId",
                        column: x => x.ExpectedIncomeSourceId,
                        principalTable: "ExpectedIncomeSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpectedIncomeSourceDates_ExpectedIncomeSourceId",
                table: "ExpectedIncomeSourceDates",
                column: "ExpectedIncomeSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpectedIncomeSourceIncomeDeductions_ExpectedIncomeSourceId",
                table: "ExpectedIncomeSourceIncomeDeductions",
                column: "ExpectedIncomeSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpectedIncomeSourceIncomeLines_ExpectedIncomeSourceId",
                table: "ExpectedIncomeSourceIncomeLines",
                column: "ExpectedIncomeSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpectedIncomeSources_AccountingPeriodId",
                table: "ExpectedIncomeSources",
                column: "AccountingPeriodId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpectedIncomeSourceDates");

            migrationBuilder.DropTable(
                name: "ExpectedIncomeSourceIncomeDeductions");

            migrationBuilder.DropTable(
                name: "ExpectedIncomeSourceIncomeLines");

            migrationBuilder.DropTable(
                name: "ExpectedIncomeSources");
        }
    }
}
