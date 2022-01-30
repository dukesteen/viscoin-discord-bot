using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Viscoin.Bot.Migrations
{
    public partial class Add_Lottery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lotteries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrizePool = table.Column<int>(type: "integer", nullable: false),
                    TicketPrice = table.Column<int>(type: "integer", nullable: false),
                    MaxTickets = table.Column<int>(type: "integer", nullable: false),
                    LotteryMessageChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LotteryMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotteries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LotteryEntry",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LotteryId = table.Column<int>(type: "integer", nullable: false),
                    TicketAmount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotteryEntry", x => new { x.UserId, x.LotteryId });
                    table.ForeignKey(
                        name: "FK_LotteryEntry_Lotteries_LotteryId",
                        column: x => x.LotteryId,
                        principalTable: "Lotteries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LotteryEntry_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LotteryEntry_LotteryId",
                table: "LotteryEntry",
                column: "LotteryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LotteryEntry");

            migrationBuilder.DropTable(
                name: "Lotteries");
        }
    }
}
