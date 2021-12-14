using Microsoft.EntityFrameworkCore.Migrations;

namespace Randomizer.Shared.Migrations
{
    public partial class BossStates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackerBossStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackerStateId = table.Column<long>(type: "INTEGER", nullable: true),
                    BossName = table.Column<string>(type: "TEXT", nullable: true),
                    Defeated = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerBossStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackerBossStates_TrackerStates_TrackerStateId",
                        column: x => x.TrackerStateId,
                        principalTable: "TrackerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackerBossStates_TrackerStateId",
                table: "TrackerBossStates",
                column: "TrackerStateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackerBossStates");
        }
    }
}
