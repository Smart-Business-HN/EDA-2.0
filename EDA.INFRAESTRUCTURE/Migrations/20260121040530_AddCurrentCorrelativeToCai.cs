using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDA.INFRAESTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentCorrelativeToCai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentCorrelative",
                table: "Cais",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCorrelative",
                table: "Cais");
        }
    }
}
