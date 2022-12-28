using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Randomizer.Shared.Migrations
{
    public partial class MultiplayerGameDetailsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MultiplayerGameUrl",
                table: "GeneratedRoms");

            migrationBuilder.RenameColumn(
                name: "MultiplayerGameType",
                table: "GeneratedRoms",
                newName: "MultiplayerGameDetailsId");

            migrationBuilder.CreateTable(
                name: "MultiplayerGames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConnectionUrl = table.Column<string>(type: "TEXT", nullable: false),
                    GameGuid = table.Column<string>(type: "TEXT", nullable: false),
                    GameUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerGuid = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerKey = table.Column<string>(type: "TEXT", nullable: false),
                    JoinedDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GeneratedRomId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiplayerGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiplayerGames_GeneratedRoms_GeneratedRomId",
                        column: x => x.GeneratedRomId,
                        principalTable: "GeneratedRoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedRoms_MultiplayerGameDetailsId",
                table: "GeneratedRoms",
                column: "MultiplayerGameDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiplayerGames_GeneratedRomId",
                table: "MultiplayerGames",
                column: "GeneratedRomId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedRoms_MultiplayerGames_MultiplayerGameDetailsId",
                table: "GeneratedRoms",
                column: "MultiplayerGameDetailsId",
                principalTable: "MultiplayerGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedRoms_MultiplayerGames_MultiplayerGameDetailsId",
                table: "GeneratedRoms");

            migrationBuilder.DropTable(
                name: "MultiplayerGames");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedRoms_MultiplayerGameDetailsId",
                table: "GeneratedRoms");

            migrationBuilder.RenameColumn(
                name: "MultiplayerGameDetailsId",
                table: "GeneratedRoms",
                newName: "MultiplayerGameType");

            migrationBuilder.AddColumn<string>(
                name: "MultiplayerGameUrl",
                table: "GeneratedRoms",
                type: "TEXT",
                nullable: true);
        }
    }
}
