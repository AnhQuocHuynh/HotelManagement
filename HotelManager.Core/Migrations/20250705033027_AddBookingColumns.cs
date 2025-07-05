using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManager.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<DateTime>(
                name: "BookingDate",
                table: "Bookings",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<int>(
                name: "BookingEmployeeId",
                table: "Bookings",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckInEmployeeID",
                table: "Bookings",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckOutEmployeeID",
                table: "Bookings",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingEmployeeId",
                table: "Bookings",
                column: "BookingEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckInEmployeeID",
                table: "Bookings",
                column: "CheckInEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckOutEmployeeID",
                table: "Bookings",
                column: "CheckOutEmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Employees_BookingEmployeeId",
                table: "Bookings",
                column: "BookingEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Employees_CheckInEmployeeID",
                table: "Bookings",
                column: "CheckInEmployeeID",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Employees_CheckOutEmployeeID",
                table: "Bookings",
                column: "CheckOutEmployeeID",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {



            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Employees_BookingEmployeeId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Employees_CheckInEmployeeID",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Employees_CheckOutEmployeeID",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingEmployeeId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CheckInEmployeeID",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CheckOutEmployeeID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingEmployeeId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CheckInEmployeeID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CheckOutEmployeeID",
                table: "Bookings");
        }
    }
}
