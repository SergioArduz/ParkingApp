using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAccumulatedMinutesToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccumulatedMinutes",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccumulatedMinutes",
                table: "Vehicles");
        }
    }
}
