using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Shared.Migrations
{
    /// <inheritdoc />
    public partial class SaveHintTiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackerHintStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackerStateId = table.Column<long>(type: "INTEGER", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    WorldId = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationKey = table.Column<string>(type: "TEXT", nullable: false),
                    LocationWorldId = table.Column<int>(type: "INTEGER", nullable: true),
                    LocationString = table.Column<string>(type: "TEXT", nullable: true),
                    Usefulness = table.Column<int>(type: "INTEGER", nullable: true),
                    MedallionType = table.Column<byte>(type: "INTEGER", nullable: true),
                    HintTileCode = table.Column<string>(type: "TEXT", nullable: false),
                    HintState = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerHintStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackerHintStates_TrackerStates_TrackerStateId",
                        column: x => x.TrackerStateId,
                        principalTable: "TrackerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackerHintStates_TrackerStateId",
                table: "TrackerHintStates",
                column: "TrackerStateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackerHintStates");
        }
    }
}
