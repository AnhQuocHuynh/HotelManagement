using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManager.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeCCCD2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CCCD",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CCCD",
                table: "Employees");
        }
    }
}
