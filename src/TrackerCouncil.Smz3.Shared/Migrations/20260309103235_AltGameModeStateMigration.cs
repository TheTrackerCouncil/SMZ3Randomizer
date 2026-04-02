using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Smz3.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AltGameModeStateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AltGameModeState",
                table: "TrackerStates",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltGameModeState",
                table: "TrackerStates");
        }
    }
}
