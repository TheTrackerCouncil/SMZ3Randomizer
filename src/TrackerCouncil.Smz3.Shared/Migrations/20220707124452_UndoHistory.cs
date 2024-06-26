using Microsoft.EntityFrameworkCore.Migrations;

namespace TrackerCouncil.Shared.Migrations
{
    public partial class UndoHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUndone",
                table: "TrackerHistoryEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUndone",
                table: "TrackerHistoryEvents");
        }
    }
}
