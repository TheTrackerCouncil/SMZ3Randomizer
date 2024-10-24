using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Smz3.Shared.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDungeonState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegionName",
                table: "TrackerBossStates",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TrackerPrerequisiteStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackerStateId = table.Column<long>(type: "INTEGER", nullable: true),
                    RegionName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    WorldId = table.Column<int>(type: "INTEGER", nullable: false),
                    AutoTracked = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiredItem = table.Column<byte>(type: "INTEGER", nullable: false),
                    MarkedItem = table.Column<byte>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerPrerequisiteStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackerPrerequisiteStates_TrackerStates_TrackerStateId",
                        column: x => x.TrackerStateId,
                        principalTable: "TrackerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackerRewardStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackerStateId = table.Column<long>(type: "INTEGER", nullable: true),
                    RewardType = table.Column<int>(type: "INTEGER", nullable: false),
                    MarkedReward = table.Column<int>(type: "INTEGER", nullable: true),
                    HasReceivedReward = table.Column<bool>(type: "INTEGER", nullable: false),
                    RegionName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AutoTracked = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorldId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerRewardStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackerRewardStates_TrackerStates_TrackerStateId",
                        column: x => x.TrackerStateId,
                        principalTable: "TrackerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackerTreasureStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackerStateId = table.Column<long>(type: "INTEGER", nullable: true),
                    RegionName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Cleared = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoTracked = table.Column<bool>(type: "INTEGER", nullable: false),
                    RemainingTreasure = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTreasure = table.Column<int>(type: "INTEGER", nullable: false),
                    HasManuallyClearedTreasure = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorldId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerTreasureStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackerTreasureStates_TrackerStates_TrackerStateId",
                        column: x => x.TrackerStateId,
                        principalTable: "TrackerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackerPrerequisiteStates_TrackerStateId",
                table: "TrackerPrerequisiteStates",
                column: "TrackerStateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackerRewardStates_TrackerStateId",
                table: "TrackerRewardStates",
                column: "TrackerStateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackerTreasureStates_TrackerStateId",
                table: "TrackerTreasureStates",
                column: "TrackerStateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackerPrerequisiteStates");

            migrationBuilder.DropTable(
                name: "TrackerRewardStates");

            migrationBuilder.DropTable(
                name: "TrackerTreasureStates");

            migrationBuilder.DropColumn(
                name: "RegionName",
                table: "TrackerBossStates");
        }
    }
}
