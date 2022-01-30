using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viscoin.Bot.Migrations
{
    public partial class Add_TimestampToKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BalanceUpdates",
                table: "BalanceUpdates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BalanceUpdates",
                table: "BalanceUpdates",
                columns: new[] { "Id", "Timestamp" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BalanceUpdates",
                table: "BalanceUpdates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BalanceUpdates",
                table: "BalanceUpdates",
                column: "Id");
        }
    }
}
