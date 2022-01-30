using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viscoin.Bot.Migrations
{
    public partial class Modify_RemoveQuantityFromInventoryItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "InventoryPerkEntity");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "InventoryItemEntity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "InventoryPerkEntity",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "InventoryItemEntity",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
