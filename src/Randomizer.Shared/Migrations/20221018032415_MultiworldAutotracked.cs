using Microsoft.EntityFrameworkCore.Migrations;

namespace Randomizer.Shared.Migrations
{
    public partial class MultiworldAutotracked : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ignored",
                table: "TrackerLocationStates",
                newName: "Autotracked");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Autotracked",
                table: "TrackerLocationStates",
                newName: "Ignored");
        }
    }
}
