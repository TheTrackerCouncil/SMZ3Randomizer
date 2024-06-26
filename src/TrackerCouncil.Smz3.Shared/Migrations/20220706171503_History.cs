using Microsoft.EntityFrameworkCore.Migrations;

namespace TrackerCouncil.Shared.Migrations
{
    public partial class History : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackerHistoryEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackerStateId = table.Column<long>(type: "INTEGER", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: true),
                    LocationName = table.Column<string>(type: "TEXT", nullable: true),
                    ObjectName = table.Column<string>(type: "TEXT", nullable: true),
                    IsImportant = table.Column<bool>(type: "INTEGER", nullable: false),
                    Time = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerHistoryEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackerHistoryEvents_TrackerStates_TrackerStateId",
                        column: x => x.TrackerStateId,
                        principalTable: "TrackerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackerHistoryEvents_TrackerStateId",
                table: "TrackerHistoryEvents",
                column: "TrackerStateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackerHistoryEvents");
        }
    }
}
