using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDA.INFRAESTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class AddMinStockToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinStock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinStock",
                table: "Products");
        }
    }
}
