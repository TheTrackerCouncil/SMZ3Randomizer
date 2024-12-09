using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackerCouncil.Smz3.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftedItemCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GiftedItemCount",
                table: "TrackerStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiftedItemCount",
                table: "TrackerStates");
        }
    }
}
