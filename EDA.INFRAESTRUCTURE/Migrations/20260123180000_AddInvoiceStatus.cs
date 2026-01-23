using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EDA.INFRAESTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear tabla InvoiceStatuses
            migrationBuilder.CreateTable(
                name: "InvoiceStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceStatuses", x => x.Id);
                });

            // Seed data para InvoiceStatuses
            migrationBuilder.InsertData(
                table: "InvoiceStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Creada" },
                    { 2, "Pagada" },
                    { 3, "Anulada" }
                });

            // Agregar columna StatusId a Invoices con valor por defecto 2 (Pagada)
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 2);

            // Crear índice y foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StatusId",
                table: "Invoices",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_InvoiceStatuses_StatusId",
                table: "Invoices",
                column: "StatusId",
                principalTable: "InvoiceStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_InvoiceStatuses_StatusId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_StatusId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "InvoiceStatuses");
        }
    }
}
