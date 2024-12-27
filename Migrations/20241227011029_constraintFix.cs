using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelApp.Migrations
{
    /// <inheritdoc />
    public partial class constraintFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_StartBeforeEnd",
                table: "Booking");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_StartBeforeEnd",
                table: "Booking",
                sql: "Start_Date < End_Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_StartBeforeEnd",
                table: "Booking");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_StartBeforeEnd",
                table: "Booking",
                sql: "Start_Date < Start_Date");
        }
    }
}
