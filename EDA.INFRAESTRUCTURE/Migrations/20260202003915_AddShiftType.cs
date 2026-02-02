using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDA.INFRAESTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShiftType",
                table: "Shifts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShiftType",
                table: "Shifts");
        }
    }
}
