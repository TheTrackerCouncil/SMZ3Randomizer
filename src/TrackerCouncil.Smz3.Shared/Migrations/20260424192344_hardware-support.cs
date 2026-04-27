using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Smz3.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddHardwareSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HardwarePath",
                table: "GeneratedRoms",
                type: "TEXT",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HardwarePath",
                table: "GeneratedRoms");
        }
    }
}
