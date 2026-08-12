using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationalIncomeBreakdowns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Source_IncomeId",
                table: "Transactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IncomeBreakdowns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpectedIncomeSourceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IncomeType = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    TrackedAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    UntrackedAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    PayPeriodsPerYear = table.Column<int>(type: "INTEGER", nullable: true),
                    StateIncomeStateCode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeBreakdowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeBreakdowns_ExpectedIncomeSources_ExpectedIncomeSourceId",
                        column: x => x.ExpectedIncomeSourceId,
                        principalTable: "ExpectedIncomeSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePayrollDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Disposition = table.Column<int>(type: "INTEGER", nullable: false),
                    ReducesTaxableWagesFor = table.Column<int>(type: "INTEGER", nullable: false),
                    IncomeBreakdownId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePayrollDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePayrollDeductions_IncomeBreakdowns_IncomeBreakdownId",
                        column: x => x.IncomeBreakdownId,
                        principalTable: "IncomeBreakdowns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployerContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeBreakdownId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployerContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployerContributions_IncomeBreakdowns_IncomeBreakdownId",
                        column: x => x.IncomeBreakdownId,
                        principalTable: "IncomeBreakdowns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollEarnings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeBreakdownId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollEarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollEarnings_IncomeBreakdowns_IncomeBreakdownId",
                        column: x => x.IncomeBreakdownId,
                        principalTable: "IncomeBreakdowns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollTaxWithholdings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: false),
                    SubdivisionCode = table.Column<string>(type: "TEXT", nullable: true),
                    Locality = table.Column<string>(type: "TEXT", nullable: true),
                    TaxType = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncomeBreakdownId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollTaxWithholdings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollTaxWithholdings_IncomeBreakdowns_IncomeBreakdownId",
                        column: x => x.IncomeBreakdownId,
                        principalTable: "IncomeBreakdowns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.DropTable(
                name: "ExpectedIncomeSourceIncomeDeductions");

            migrationBuilder.DropTable(
                name: "ExpectedIncomeSourceIncomeLines");

            migrationBuilder.DropTable(
                name: "IncomeTransactionIncomeDeductions");

            migrationBuilder.DropTable(
                name: "IncomeTransactionIncomeLines");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Source_IncomeId",
                table: "Transactions",
                column: "Source_IncomeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayrollDeductions_IncomeBreakdownId",
                table: "EmployeePayrollDeductions",
                column: "IncomeBreakdownId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployerContributions_IncomeBreakdownId",
                table: "EmployerContributions",
                column: "IncomeBreakdownId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeBreakdowns_ExpectedIncomeSourceId",
                table: "IncomeBreakdowns",
                column: "ExpectedIncomeSourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEarnings_IncomeBreakdownId",
                table: "PayrollEarnings",
                column: "IncomeBreakdownId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollTaxWithholdings_IncomeBreakdownId",
                table: "PayrollTaxWithholdings",
                column: "IncomeBreakdownId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_IncomeBreakdowns_Source_IncomeId",
                table: "Transactions",
                column: "Source_IncomeId",
                principalTable: "IncomeBreakdowns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_IncomeBreakdowns_Source_IncomeId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "EmployerContributions");

            migrationBuilder.DropTable(
                name: "PayrollTaxWithholdings");

            migrationBuilder.DropTable(
                name: "EmployeePayrollDeductions");

            migrationBuilder.DropTable(
                name: "PayrollEarnings");

            migrationBuilder.DropTable(
                name: "IncomeBreakdowns");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_Source_IncomeId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Source_IncomeId",
                table: "Transactions");

            migrationBuilder.CreateTable(
                name: "ExpectedIncomeSourceIncomeDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
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
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "IncomeTransactionIncomeDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "IncomeTransactionIncomeLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_ExpectedIncomeSourceIncomeDeductions_ExpectedIncomeSourceId",
                table: "ExpectedIncomeSourceIncomeDeductions",
                column: "ExpectedIncomeSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpectedIncomeSourceIncomeLines_ExpectedIncomeSourceId",
                table: "ExpectedIncomeSourceIncomeLines",
                column: "ExpectedIncomeSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTransactionIncomeDeductions_IncomeTransactionId",
                table: "IncomeTransactionIncomeDeductions",
                column: "IncomeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTransactionIncomeLines_IncomeTransactionId",
                table: "IncomeTransactionIncomeLines",
                column: "IncomeTransactionId");
        }
    }
}