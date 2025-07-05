using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManager.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm các cột bị thiếu cho MaintenanceReports
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "MaintenanceReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "MaintenanceReports",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");



            // Thêm cột RoomStatus cho Rooms
            migrationBuilder.AddColumn<int>(
                name: "RoomStatus",
                table: "Rooms",
                nullable: false,
                defaultValue: 0); // Giả sử RoomStatus là enum



        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "MaintenanceReports");

            migrationBuilder.DropColumn(
            name: "Description",
            table: "MaintenanceReports");


            migrationBuilder.DropColumn(
                name: "RoomStatus",
                table: "Rooms");
        }
    }
}
