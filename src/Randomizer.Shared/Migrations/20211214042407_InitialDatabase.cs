using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Randomizer.Shared.Migrations
{
    public partial class InitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS ""GeneratedRoms"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_GeneratedRoms"" PRIMARY KEY AUTOINCREMENT,
                ""Date"" TEXT NOT NULL,
                ""Label"" TEXT NULL,
                ""Seed"" TEXT NULL,
                ""Settings"" TEXT NULL,
                ""GeneratorVersion"" INTEGER NOT NULL,
                ""RomPath"" TEXT NULL,
                ""SpoilerPath"" TEXT NULL,
                ""TrackerStateId"" INTEGER NULL,
                CONSTRAINT ""FK_GeneratedRoms_TrackerStates_TrackerStateId"" FOREIGN KEY (""TrackerStateId"") REFERENCES ""TrackerStates"" (""Id"") ON DELETE RESTRICT
            )");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS ""TrackerStates"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_TrackerStates"" PRIMARY KEY AUTOINCREMENT,
                ""StartDateTime"" TEXT NOT NULL,
                ""UpdatedDateTime"" TEXT NOT NULL,
                ""SecondsElapsed"" REAL NOT NULL,
                ""PercentageCleared"" INTEGER NOT NULL
            )");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS ""TrackerLocationStates"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_TrackerLocationStates"" PRIMARY KEY AUTOINCREMENT,
                ""TrackerStateId"" INTEGER NULL,
                ""LocationId"" INTEGER NOT NULL,
                ""Item"" INTEGER NULL,
                ""Cleared"" INTEGER NOT NULL,
                CONSTRAINT ""FK_TrackerLocationStates_TrackerStates_TrackerStateId"" FOREIGN KEY (""TrackerStateId"") REFERENCES ""TrackerStates"" (""Id"") ON DELETE CASCADE
            )");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS ""TrackerRegionStates"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_TrackerRegionStates"" PRIMARY KEY AUTOINCREMENT,
                ""TrackerStateId"" INTEGER NULL,
                ""TypeName"" TEXT NULL,
                ""Reward"" INTEGER NULL,
                ""Medallion"" INTEGER NULL,
                CONSTRAINT ""FK_TrackerRegionStates_TrackerStates_TrackerStateId"" FOREIGN KEY (""TrackerStateId"") REFERENCES ""TrackerStates"" (""Id"") ON DELETE CASCADE
            )");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS ""TrackerDungeonStates"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_TrackerDungeonStates"" PRIMARY KEY AUTOINCREMENT,
                ""TrackerStateId"" INTEGER NULL,
                ""Name"" TEXT NULL,
                ""Cleared"" INTEGER NOT NULL,
                ""RemainingTreasure"" INTEGER NOT NULL,
                ""Reward"" INTEGER NOT NULL,
                ""RequiredMedallion"" INTEGER NOT NULL,
                CONSTRAINT ""FK_TrackerDungeonStates_TrackerStates_TrackerStateId"" FOREIGN KEY (""TrackerStateId"") REFERENCES ""TrackerStates"" (""Id"") ON DELETE CASCADE
            )");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS ""TrackerItemStates"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_TrackerItemStates"" PRIMARY KEY AUTOINCREMENT,
                ""TrackerStateId"" INTEGER NULL,
                ""ItemName"" TEXT NULL,
                ""TrackingState"" INTEGER NOT NULL,
                CONSTRAINT ""FK_TrackerItemStates_TrackerStates_TrackerStateId"" FOREIGN KEY (""TrackerStateId"") REFERENCES ""TrackerStates"" (""Id"") ON DELETE CASCADE
            )");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS ""TrackerMarkedLocations"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_TrackerMarkedLocations"" PRIMARY KEY AUTOINCREMENT,
                ""TrackerStateId"" INTEGER NULL,
                ""LocationId"" INTEGER NOT NULL,
                ""ItemName"" TEXT NULL,
                CONSTRAINT ""FK_TrackerMarkedLocations_TrackerStates_TrackerStateId"" FOREIGN KEY (""TrackerStateId"") REFERENCES ""TrackerStates"" (""Id"") ON DELETE CASCADE
            )");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_GeneratedRoms_TrackerStateId"" ON ""GeneratedRoms"" (""TrackerStateId"")");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_TrackerDungeonStates_TrackerStateId"" ON ""TrackerDungeonStates"" (""TrackerStateId"")");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_TrackerItemStates_TrackerStateId"" ON ""TrackerItemStates"" (""TrackerStateId"")");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_TrackerLocationStates_TrackerStateId"" ON ""TrackerLocationStates"" (""TrackerStateId"")");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_TrackerMarkedLocations_TrackerStateId"" ON ""TrackerMarkedLocations"" (""TrackerStateId"")");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_TrackerRegionStates_TrackerStateId"" ON ""TrackerRegionStates"" (""TrackerStateId"")");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedRoms");

            migrationBuilder.DropTable(
                name: "TrackerDungeonStates");

            migrationBuilder.DropTable(
                name: "TrackerItemStates");

            migrationBuilder.DropTable(
                name: "TrackerLocationStates");

            migrationBuilder.DropTable(
                name: "TrackerMarkedLocations");

            migrationBuilder.DropTable(
                name: "TrackerRegionStates");

            migrationBuilder.DropTable(
                name: "TrackerStates");
        }
    }
}
