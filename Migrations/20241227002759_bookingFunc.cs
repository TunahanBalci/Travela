using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelApp.Migrations
{
    /// <inheritdoc />
    public partial class bookingFunc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Accommodations_AccommodationID",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserID",
                table: "Bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "Bookings",
                newName: "Booking");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_UserID",
                table: "Booking",
                newName: "IX_Booking_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_AccommodationID",
                table: "Booking",
                newName: "IX_Booking_AccommodationID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Booking",
                table: "Booking",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Accommodations_AccommodationID",
                table: "Booking",
                column: "AccommodationID",
                principalTable: "Accommodations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Users_UserID",
                table: "Booking",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Accommodations_AccommodationID",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Users_UserID",
                table: "Booking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Booking",
                table: "Booking");

            migrationBuilder.RenameTable(
                name: "Booking",
                newName: "Bookings");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_UserID",
                table: "Bookings",
                newName: "IX_Bookings_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_AccommodationID",
                table: "Bookings",
                newName: "IX_Bookings_AccommodationID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Accommodations_AccommodationID",
                table: "Bookings",
                column: "AccommodationID",
                principalTable: "Accommodations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserID",
                table: "Bookings",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
