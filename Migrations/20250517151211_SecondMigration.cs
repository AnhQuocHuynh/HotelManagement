using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManager.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDetails_Rooms_RoomNumber",
                table: "InvoiceDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDetails_Rooms_RoomNumber",
                table: "InvoiceDetails",
                column: "RoomNumber",
                principalTable: "Rooms",
                principalColumn: "RoomNumber",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDetails_Rooms_RoomNumber",
                table: "InvoiceDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDetails_Rooms_RoomNumber",
                table: "InvoiceDetails",
                column: "RoomNumber",
                principalTable: "Rooms",
                principalColumn: "RoomNumber");
        }
    }
}
