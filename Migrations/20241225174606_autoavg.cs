using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelApp.Migrations
{
    /// <inheritdoc />
    public partial class autoavg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Average_Rating",
                table: "Destinations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Average_Rating",
                table: "Activities",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Average_Rating",
                table: "Accommodations",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Average_Rating",
                table: "Destinations");

            migrationBuilder.DropColumn(
                name: "Average_Rating",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Average_Rating",
                table: "Accommodations");
        }
    }
}
