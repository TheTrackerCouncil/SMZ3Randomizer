using Microsoft.EntityFrameworkCore.Migrations;

namespace Randomizer.Shared.Migrations
{
    public partial class MultiplayerGameDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MultiplayerGameType",
                table: "GeneratedRoms",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MultiplayerGameUrl",
                table: "GeneratedRoms",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MultiplayerGameType",
                table: "GeneratedRoms");

            migrationBuilder.DropColumn(
                name: "MultiplayerGameUrl",
                table: "GeneratedRoms");
        }
    }
}
