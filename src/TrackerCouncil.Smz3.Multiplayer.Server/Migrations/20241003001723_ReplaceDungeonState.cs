using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Smz3.Multiplayer.Server.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDungeonState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MultiplayerDungeonStates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MultiplayerDungeonStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    Dungeon = table.Column<string>(type: "TEXT", nullable: false),
                    Tracked = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrackedTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiplayerDungeonStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiplayerDungeonStates_MultiplayerGameStates_GameId",
                        column: x => x.GameId,
                        principalTable: "MultiplayerGameStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MultiplayerDungeonStates_MultiplayerPlayerStates_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "MultiplayerPlayerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerDungeonStates_GameId",
                table: "MultiplayerDungeonStates",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerDungeonStates_PlayerId",
                table: "MultiplayerDungeonStates",
                column: "PlayerId");
        }
    }
}
