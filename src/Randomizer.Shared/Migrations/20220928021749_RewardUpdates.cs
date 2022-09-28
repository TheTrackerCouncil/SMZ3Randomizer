using Microsoft.EntityFrameworkCore.Migrations;

namespace Randomizer.Shared.Migrations
{
    public partial class RewardUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Reward",
                table: "TrackerDungeonStates",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte>(
                name: "RequiredMedallion",
                table: "TrackerDungeonStates",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "HasManuallyClearedTreasure",
                table: "TrackerDungeonStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "MarkedMedallion",
                table: "TrackerDungeonStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarkedReward",
                table: "TrackerDungeonStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TrackerBossStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasManuallyClearedTreasure",
                table: "TrackerDungeonStates");

            migrationBuilder.DropColumn(
                name: "MarkedMedallion",
                table: "TrackerDungeonStates");

            migrationBuilder.DropColumn(
                name: "MarkedReward",
                table: "TrackerDungeonStates");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TrackerBossStates");

            migrationBuilder.AlterColumn<int>(
                name: "Reward",
                table: "TrackerDungeonStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RequiredMedallion",
                table: "TrackerDungeonStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(byte),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
