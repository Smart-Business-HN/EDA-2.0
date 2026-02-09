using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDA.INFRAESTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiCashRegisterAndPrinterConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CashRegisterId",
                table: "Shifts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CashRegisterId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrinted",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PrintCount",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrintedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrinterConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrinterType = table.Column<int>(type: "int", nullable: false),
                    PrinterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FontSize = table.Column<int>(type: "int", nullable: false, defaultValue: 8),
                    CopyStrategy = table.Column<int>(type: "int", nullable: false),
                    CopiesCount = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PrintWidth = table.Column<int>(type: "int", nullable: false, defaultValue: 80),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrinterConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashRegisters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrinterConfigurationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegisters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashRegisters_PrinterConfigurations_PrinterConfigurationId",
                        column: x => x.PrinterConfigurationId,
                        principalTable: "PrinterConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ExpenseAccounts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Servicios Publicos");

            migrationBuilder.InsertData(
                table: "PrinterConfigurations",
                columns: new[] { "Id", "CopiesCount", "CopyStrategy", "CreationDate", "FontSize", "IsActive", "Name", "PrintWidth", "PrinterName", "PrinterType" },
                values: new object[] { 1, 2, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, true, "Impresora Termica 80mm", 80, null, 1 });

            migrationBuilder.InsertData(
                table: "CashRegisters",
                columns: new[] { "Id", "Code", "CreationDate", "IsActive", "Name", "PrinterConfigurationId" },
                values: new object[] { 1, "C001", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Caja Principal", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_CashRegisterId",
                table: "Shifts",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CashRegisterId",
                table: "Invoices",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_PrinterConfigurationId",
                table: "CashRegisters",
                column: "PrinterConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_CashRegisters_CashRegisterId",
                table: "Invoices",
                column: "CashRegisterId",
                principalTable: "CashRegisters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_CashRegisters_CashRegisterId",
                table: "Shifts",
                column: "CashRegisterId",
                principalTable: "CashRegisters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_CashRegisters_CashRegisterId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_CashRegisters_CashRegisterId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "CashRegisters");

            migrationBuilder.DropTable(
                name: "PrinterConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_CashRegisterId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CashRegisterId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CashRegisterId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "CashRegisterId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsPrinted",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PrintCount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PrintedAt",
                table: "Invoices");

            migrationBuilder.UpdateData(
                table: "ExpenseAccounts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Servicios P�blicos");
        }
    }
}
