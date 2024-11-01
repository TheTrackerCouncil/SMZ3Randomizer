using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Smz3.Shared.Migrations
{
    /// <inheritdoc />
    public partial class StoreLocationUsefulness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoTracked",
                table: "TrackerTreasureStates");

            migrationBuilder.DropColumn(
                name: "Cleared",
                table: "TrackerTreasureStates");

            migrationBuilder.AddColumn<int>(
                name: "MarkedUsefulness",
                table: "TrackerLocationStates",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkedUsefulness",
                table: "TrackerLocationStates");

            migrationBuilder.AddColumn<bool>(
                name: "AutoTracked",
                table: "TrackerTreasureStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Cleared",
                table: "TrackerTreasureStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
