using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDA.INFRAESTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceCreditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditDays",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OutstandingAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditDays",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "OutstandingAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Invoices");
        }
    }
}
