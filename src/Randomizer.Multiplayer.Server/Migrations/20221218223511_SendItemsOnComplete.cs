using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Randomizer.Multiplayer.Server.Migrations
{
    public partial class SendItemsOnComplete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SendItemsOnComplete",
                table: "MultiplayerGameStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendItemsOnComplete",
                table: "MultiplayerGameStates");
        }
    }
}
