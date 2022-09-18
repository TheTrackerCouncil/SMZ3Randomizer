using Microsoft.EntityFrameworkCore.Migrations;

namespace Randomizer.Shared.Migrations
{
    public partial class DataProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "MarkedItem",
                table: "TrackerLocationStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "TrackerItemStates",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkedItem",
                table: "TrackerLocationStates");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TrackerItemStates");
        }
    }
}
