using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelApp.Migrations
{
    /// <inheritdoc />
    public partial class userTravelHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users_Accommodation",
                table: "Users_Accommodation");

            migrationBuilder.RenameTable(
                name: "Users_Accommodation",
                newName: "Visits");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Visits",
                table: "Visits",
                columns: new[] { "UserID", "DestinationID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Visits",
                table: "Visits");

            migrationBuilder.RenameTable(
                name: "Visits",
                newName: "Users_Accommodation");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users_Accommodation",
                table: "Users_Accommodation",
                columns: new[] { "UserID", "DestinationID" });
        }
    }
}
