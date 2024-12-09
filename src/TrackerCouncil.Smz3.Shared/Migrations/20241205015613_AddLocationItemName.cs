using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Smz3.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationItemName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerName",
                table: "TrackerItemStates");

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "TrackerLocationStates",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemOwnerName",
                table: "TrackerLocationStates",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "TrackerLocationStates");

            migrationBuilder.DropColumn(
                name: "ItemOwnerName",
                table: "TrackerLocationStates");

            migrationBuilder.AddColumn<string>(
                name: "PlayerName",
                table: "TrackerItemStates",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }
    }
}
