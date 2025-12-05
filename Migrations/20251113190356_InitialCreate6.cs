using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTA_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SentEmailForCurrentAllocation",
                table: "Allocations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentEmailForCurrentAllocation",
                table: "Allocations");
        }
    }
}
