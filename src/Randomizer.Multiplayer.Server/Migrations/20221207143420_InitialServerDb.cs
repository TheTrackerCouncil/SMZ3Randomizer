using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Randomizer.Multiplayer.Server.Migrations
{
    public partial class InitialServerDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MultiplayerGameStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastMessage = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Seed = table.Column<string>(type: "TEXT", nullable: false),
                    ValidationHash = table.Column<string>(type: "TEXT", nullable: false),
                    SaveToDatabase = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiplayerGameStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MultiplayerPlayerStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false),
                    Guid = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneticName = table.Column<string>(type: "TEXT", nullable: false),
                    WorldId = table.Column<int>(type: "INTEGER", nullable: true),
                    Config = table.Column<string>(type: "TEXT", nullable: true),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AdditionalData = table.Column<string>(type: "TEXT", nullable: true),
                    GenerationData = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiplayerPlayerStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiplayerPlayerStates_MultiplayerGameStates_GameId",
                        column: x => x.GameId,
                        principalTable: "MultiplayerGameStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiplayerBossStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    Boss = table.Column<int>(type: "INTEGER", nullable: false),
                    Tracked = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrackedTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiplayerBossStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiplayerBossStates_MultiplayerGameStates_GameId",
                        column: x => x.GameId,
                        principalTable: "MultiplayerGameStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MultiplayerBossStates_MultiplayerPlayerStates_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "MultiplayerPlayerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "MultiplayerItemStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    Item = table.Column<byte>(type: "INTEGER", nullable: false),
                    TrackingValue = table.Column<int>(type: "INTEGER", nullable: false),
                    TrackedTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiplayerItemStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiplayerItemStates_MultiplayerGameStates_GameId",
                        column: x => x.GameId,
                        principalTable: "MultiplayerGameStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MultiplayerItemStates_MultiplayerPlayerStates_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "MultiplayerPlayerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiplayerLocationStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<long>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tracked = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrackedTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiplayerLocationStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiplayerLocationStates_MultiplayerGameStates_GameId",
                        column: x => x.GameId,
                        principalTable: "MultiplayerGameStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MultiplayerLocationStates_MultiplayerPlayerStates_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "MultiplayerPlayerStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerBossStates_GameId",
                table: "MultiplayerBossStates",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerBossStates_PlayerId",
                table: "MultiplayerBossStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerDungeonStates_GameId",
                table: "MultiplayerDungeonStates",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerDungeonStates_PlayerId",
                table: "MultiplayerDungeonStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerItemStates_GameId",
                table: "MultiplayerItemStates",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerItemStates_PlayerId",
                table: "MultiplayerItemStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerLocationStates_GameId",
                table: "MultiplayerLocationStates",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerLocationStates_PlayerId",
                table: "MultiplayerLocationStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerPlayerStates_GameId",
                table: "MultiplayerPlayerStates",
                column: "GameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MultiplayerBossStates");

            migrationBuilder.DropTable(
                name: "MultiplayerDungeonStates");

            migrationBuilder.DropTable(
                name: "MultiplayerItemStates");

            migrationBuilder.DropTable(
                name: "MultiplayerLocationStates");

            migrationBuilder.DropTable(
                name: "MultiplayerPlayerStates");

            migrationBuilder.DropTable(
                name: "MultiplayerGameStates");
        }
    }
}
