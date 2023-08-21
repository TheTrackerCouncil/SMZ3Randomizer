using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Randomizer.Shared.Migrations
{
    /// <inheritdoc />
    public partial class MsuPaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MsuPaths",
                table: "GeneratedRoms",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MsuPaths",
                table: "GeneratedRoms");
        }
    }
}
