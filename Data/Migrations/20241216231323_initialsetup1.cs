using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class initialsetup1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    City_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cost_Of_Living = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Climate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Terrain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.City_ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    User_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.User_ID);
                });

            migrationBuilder.CreateTable(
                name: "Accommodations",
                columns: table => new
                {
                    Accommodation_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Availability = table.Column<int>(type: "int", nullable: false),
                    Average_Review = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    City_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accommodations", x => x.Accommodation_ID);
                    table.ForeignKey(
                        name: "FK_Accommodations_Cities_City_ID",
                        column: x => x.City_ID,
                        principalTable: "Cities",
                        principalColumn: "City_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CityCommonActivities",
                columns: table => new
                {
                    City_ID = table.Column<int>(type: "int", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityCommonActivities", x => new { x.City_ID, x.Activity });
                    table.ForeignKey(
                        name: "FK_CityCommonActivities_Cities_City_ID",
                        column: x => x.City_ID,
                        principalTable: "Cities",
                        principalColumn: "City_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Destination_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Average_Review = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    City_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Destination_ID);
                    table.ForeignKey(
                        name: "FK_Destinations_Cities_City_ID",
                        column: x => x.City_ID,
                        principalTable: "Cities",
                        principalColumn: "City_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Review_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_ID = table.Column<int>(type: "int", nullable: false),
                    Entity_Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Entity_ID = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Review_ID);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_User_ID",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "User_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    User_ID = table.Column<int>(type: "int", nullable: false),
                    Preference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => new { x.User_ID, x.Preference });
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_User_ID",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "User_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Booking_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_ID = table.Column<int>(type: "int", nullable: false),
                    Accommodation_ID = table.Column<int>(type: "int", nullable: false),
                    Start_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Booking_Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Booking_ID);
                    table.ForeignKey(
                        name: "FK_Bookings_Accommodations_Accommodation_ID",
                        column: x => x.Accommodation_ID,
                        principalTable: "Accommodations",
                        principalColumn: "Accommodation_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_User_ID",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "User_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Activity_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Schedule = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Required_Reservations = table.Column<bool>(type: "bit", nullable: false),
                    Average_Review = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    Destination_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Activity_ID);
                    table.ForeignKey(
                        name: "FK_Activities_Destinations_Destination_ID",
                        column: x => x.Destination_ID,
                        principalTable: "Destinations",
                        principalColumn: "Destination_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DestinationAttractions",
                columns: table => new
                {
                    Destination_ID = table.Column<int>(type: "int", nullable: false),
                    Attraction = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationAttractions", x => new { x.Destination_ID, x.Attraction });
                    table.ForeignKey(
                        name: "FK_DestinationAttractions_Destinations_Destination_ID",
                        column: x => x.Destination_ID,
                        principalTable: "Destinations",
                        principalColumn: "Destination_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDestinations",
                columns: table => new
                {
                    User_ID = table.Column<int>(type: "int", nullable: false),
                    Destination_ID = table.Column<int>(type: "int", nullable: false),
                    Is_Favorite = table.Column<bool>(type: "bit", nullable: false),
                    Visited = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDestinations", x => new { x.User_ID, x.Destination_ID });
                    table.ForeignKey(
                        name: "FK_UserDestinations_Destinations_Destination_ID",
                        column: x => x.Destination_ID,
                        principalTable: "Destinations",
                        principalColumn: "Destination_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDestinations_Users_User_ID",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "User_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTravelHistories",
                columns: table => new
                {
                    User_ID = table.Column<int>(type: "int", nullable: false),
                    Destination_ID = table.Column<int>(type: "int", nullable: false),
                    Visit_Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTravelHistories", x => new { x.User_ID, x.Destination_ID });
                    table.ForeignKey(
                        name: "FK_UserTravelHistories_Destinations_Destination_ID",
                        column: x => x.Destination_ID,
                        principalTable: "Destinations",
                        principalColumn: "Destination_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTravelHistories_Users_User_ID",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "User_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewComments",
                columns: table => new
                {
                    Comment_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Review_ID = table.Column<int>(type: "int", nullable: false),
                    Comment_Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Comment_Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewComments", x => x.Comment_ID);
                    table.ForeignKey(
                        name: "FK_ReviewComments_Reviews_Review_ID",
                        column: x => x.Review_ID,
                        principalTable: "Reviews",
                        principalColumn: "Review_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationActivities",
                columns: table => new
                {
                    Accommodation_ID = table.Column<int>(type: "int", nullable: false),
                    Activity_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationActivities", x => new { x.Accommodation_ID, x.Activity_ID });
                    table.ForeignKey(
                        name: "FK_AccommodationActivities_Accommodations_Accommodation_ID",
                        column: x => x.Accommodation_ID,
                        principalTable: "Accommodations",
                        principalColumn: "Accommodation_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccommodationActivities_Activities_Activity_ID",
                        column: x => x.Activity_ID,
                        principalTable: "Activities",
                        principalColumn: "Activity_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationActivities_Activity_ID",
                table: "AccommodationActivities",
                column: "Activity_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_City_ID",
                table: "Accommodations",
                column: "City_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Destination_ID",
                table: "Activities",
                column: "Destination_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Accommodation_ID",
                table: "Bookings",
                column: "Accommodation_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_User_ID",
                table: "Bookings",
                column: "User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_City_ID",
                table: "Destinations",
                column: "City_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComments_Review_ID",
                table: "ReviewComments",
                column: "Review_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_User_ID",
                table: "Reviews",
                column: "User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_UserDestinations_Destination_ID",
                table: "UserDestinations",
                column: "Destination_ID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTravelHistories_Destination_ID",
                table: "UserTravelHistories",
                column: "Destination_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationActivities");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "CityCommonActivities");

            migrationBuilder.DropTable(
                name: "DestinationAttractions");

            migrationBuilder.DropTable(
                name: "ReviewComments");

            migrationBuilder.DropTable(
                name: "UserDestinations");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "UserTravelHistories");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Accommodations");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
