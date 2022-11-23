using Microsoft.EntityFrameworkCore.Migrations;

namespace Randomizer.Shared.Migrations
{
    public partial class MultiplayerFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMultiplayer",
                table: "GeneratedRoms",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMultiplayer",
                table: "GeneratedRoms");
        }
    }
}
