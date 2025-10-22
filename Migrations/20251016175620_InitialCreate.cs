using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TTA_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    HolidayDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.HolidayDate);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartureLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrivalLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TripPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AllocateForCurrentMonth = table.Column<bool>(type: "bit", nullable: false),
                    UserListViewEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SettingsId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SendEmail = table.Column<bool>(type: "bit", nullable: false),
                    Pin = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Allocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllocationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookerUserId = table.Column<int>(type: "int", nullable: false),
                    TripType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Allocations_Users_BookerUserId",
                        column: x => x.BookerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllocationTravelers",
                columns: table => new
                {
                    AllocationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationTravelers", x => new { x.AllocationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AllocationTravelers_Allocations_AllocationId",
                        column: x => x.AllocationId,
                        principalTable: "Allocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllocationTravelers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanSelectedDays",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanSelectedDays", x => new { x.PlanId, x.Day });
                    table.ForeignKey(
                        name: "FK_PlanSelectedDays_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "SettingsId", "AllocateForCurrentMonth", "ArrivalLabel", "DepartureLabel", "TripPrice", "UserListViewEnabled" },
                values: new object[] { 1, true, "Arrival", "Departure", 240.00m, true });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "Pin", "Role", "SendEmail" },
                values: new object[,]
                {
                    { 1, "alloc-admin@trip.com", "Allocation Admin", "0000", 0, true },
                    { 2, "sys-admin@trip.com", "System Admin", "0000", 2, true },
                    { 3, "gayathri@trip.com", "Gayathri", "1001", 1, true },
                    { 4, "gokul@trip.com", "Gokul", "1001", 1, true },
                    { 5, "kiruthika@trip.com", "Kiruthika", "1001", 1, true },
                    { 6, "narendran@trip.com", "Narendran", "1001", 1, true },
                    { 7, "navin@trip.com", "Navin", "1001", 1, true },
                    { 8, "sangeetha@trip.com", "Sangeetha", "1001", 1, true },
                    { 9, "sarath@trip.com", "Sarath", "1001", 1, true },
                    { 10, "shalini@trip.com", "Shalini", "1001", 1, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_BookerUserId",
                table: "Allocations",
                column: "BookerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationTravelers_UserId",
                table: "AllocationTravelers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_UserId",
                table: "Plans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllocationTravelers");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "PlanSelectedDays");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "Allocations");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
