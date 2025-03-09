using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Smz3.Shared.Migrations
{
    /// <inheritdoc />
    public partial class RandomGoalAmounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GanonCrystalCount",
                table: "TrackerStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GanonsTowerCrystalCount",
                table: "TrackerStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarkedGanonCrystalCount",
                table: "TrackerStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarkedGanonsTowerCrystalCount",
                table: "TrackerStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarkedTourianBossCount",
                table: "TrackerStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TourianBossCount",
                table: "TrackerStates",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GanonCrystalCount",
                table: "TrackerStates");

            migrationBuilder.DropColumn(
                name: "GanonsTowerCrystalCount",
                table: "TrackerStates");

            migrationBuilder.DropColumn(
                name: "MarkedGanonCrystalCount",
                table: "TrackerStates");

            migrationBuilder.DropColumn(
                name: "MarkedGanonsTowerCrystalCount",
                table: "TrackerStates");

            migrationBuilder.DropColumn(
                name: "MarkedTourianBossCount",
                table: "TrackerStates");

            migrationBuilder.DropColumn(
                name: "TourianBossCount",
                table: "TrackerStates");
        }
    }
}
