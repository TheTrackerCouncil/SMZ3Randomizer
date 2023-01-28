using Microsoft.EntityFrameworkCore.Migrations;

namespace Randomizer.Shared.Migrations
{
    public partial class Multiworld : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocalWorldId",
                table: "TrackerStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<byte>(
                name: "Item",
                table: "TrackerLocationStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ignored",
                table: "TrackerLocationStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ItemWorldId",
                table: "TrackerLocationStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorldId",
                table: "TrackerLocationStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorldId",
                table: "TrackerItemStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorldId",
                table: "TrackerDungeonStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorldId",
                table: "TrackerBossStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalWorldId",
                table: "TrackerStates");

            migrationBuilder.DropColumn(
                name: "Ignored",
                table: "TrackerLocationStates");

            migrationBuilder.DropColumn(
                name: "ItemWorldId",
                table: "TrackerLocationStates");

            migrationBuilder.DropColumn(
                name: "WorldId",
                table: "TrackerLocationStates");

            migrationBuilder.DropColumn(
                name: "WorldId",
                table: "TrackerItemStates");

            migrationBuilder.DropColumn(
                name: "WorldId",
                table: "TrackerDungeonStates");

            migrationBuilder.DropColumn(
                name: "WorldId",
                table: "TrackerBossStates");

            migrationBuilder.AlterColumn<byte>(
                name: "Item",
                table: "TrackerLocationStates",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "INTEGER");
        }
    }
}
