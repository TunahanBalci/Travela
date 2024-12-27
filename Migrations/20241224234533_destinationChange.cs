using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelApp.Migrations
{
    /// <inheritdoc />
    public partial class destinationChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attractions");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Preferences",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationID",
                table: "Preferences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_DestinationID",
                table: "Preferences",
                column: "DestinationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Preferences_Destinations_DestinationID",
                table: "Preferences",
                column: "DestinationID",
                principalTable: "Destinations",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Preferences_Destinations_DestinationID",
                table: "Preferences");

            migrationBuilder.DropIndex(
                name: "IX_Preferences_DestinationID",
                table: "Preferences");

            migrationBuilder.DropColumn(
                name: "DestinationID",
                table: "Preferences");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Preferences",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "Attractions",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attractions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Attractions_Destinations_DestinationID",
                        column: x => x.DestinationID,
                        principalTable: "Destinations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attractions_DestinationID",
                table: "Attractions",
                column: "DestinationID");
        }
    }
}
