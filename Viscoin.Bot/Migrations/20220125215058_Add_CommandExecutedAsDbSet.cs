using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viscoin.Bot.Migrations
{
    public partial class Add_CommandExecutedAsDbSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandExecuted_Users_UserId",
                table: "CommandExecuted");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommandExecuted",
                table: "CommandExecuted");

            migrationBuilder.RenameTable(
                name: "CommandExecuted",
                newName: "CommandsExecuted");

            migrationBuilder.RenameIndex(
                name: "IX_CommandExecuted_UserId",
                table: "CommandsExecuted",
                newName: "IX_CommandsExecuted_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommandsExecuted",
                table: "CommandsExecuted",
                columns: new[] { "Id", "TimeExecuted" });

            migrationBuilder.AddForeignKey(
                name: "FK_CommandsExecuted_Users_UserId",
                table: "CommandsExecuted",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandsExecuted_Users_UserId",
                table: "CommandsExecuted");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommandsExecuted",
                table: "CommandsExecuted");

            migrationBuilder.RenameTable(
                name: "CommandsExecuted",
                newName: "CommandExecuted");

            migrationBuilder.RenameIndex(
                name: "IX_CommandsExecuted_UserId",
                table: "CommandExecuted",
                newName: "IX_CommandExecuted_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommandExecuted",
                table: "CommandExecuted",
                columns: new[] { "Id", "TimeExecuted" });

            migrationBuilder.AddForeignKey(
                name: "FK_CommandExecuted_Users_UserId",
                table: "CommandExecuted",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
