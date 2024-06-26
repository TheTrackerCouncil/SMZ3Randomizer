using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Shared.Migrations
{
    /// <inheritdoc />
    public partial class MsuRandomizationStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MsuRandomizationStyle",
                table: "GeneratedRoms",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MsuRandomizationStyle",
                table: "GeneratedRoms");
        }
    }
}
