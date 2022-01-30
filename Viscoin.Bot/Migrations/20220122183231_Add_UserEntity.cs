using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viscoin.Bot.Migrations
{
    public partial class Add_UserEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cooldowns",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CommandName = table.Column<string>(type: "text", nullable: false),
                    LastTimeRan = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cooldowns", x => new { x.UserId, x.CommandName });
                });

            migrationBuilder.CreateTable(
                name: "GamblingChannels",
                columns: table => new
                {
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamblingChannels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "ItemEntities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(50)", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Cost = table.Column<int>(type: "integer", nullable: false),
                    MaxQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerkEntities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(50)", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Cost = table.Column<int>(type: "integer", nullable: false),
                    MaxQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerkEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeedHashes",
                columns: table => new
                {
                    ServerSeed = table.Column<string>(type: "text", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedHashes", x => x.ServerSeed);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Balance = table.Column<int>(type: "integer", nullable: false),
                    ServerSeed = table.Column<Guid>(type: "uuid", nullable: false),
                    NextServerSeed = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientSeed = table.Column<string>(type: "varchar(50)", nullable: false),
                    Nonce = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WordleEntries",
                columns: table => new
                {
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    WordleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordleEntries", x => new { x.DiscordId, x.WordleId });
                });

            migrationBuilder.CreateTable(
                name: "InventoryEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEntityId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryEntity_Users_UserEntityId",
                        column: x => x.UserEntityId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItemEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<string>(type: "varchar(50)", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItemEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItemEntity_InventoryEntity_InventoryEntityId",
                        column: x => x.InventoryEntityId,
                        principalTable: "InventoryEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryItemEntity_ItemEntities_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ItemEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryPerkEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerkId = table.Column<string>(type: "varchar(50)", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryPerkEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryPerkEntity_InventoryEntity_InventoryEntityId",
                        column: x => x.InventoryEntityId,
                        principalTable: "InventoryEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryPerkEntity_PerkEntities_PerkId",
                        column: x => x.PerkId,
                        principalTable: "PerkEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryEntity_UserEntityId",
                table: "InventoryEntity",
                column: "UserEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItemEntity_InventoryEntityId",
                table: "InventoryItemEntity",
                column: "InventoryEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItemEntity_ItemId",
                table: "InventoryItemEntity",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryPerkEntity_InventoryEntityId",
                table: "InventoryPerkEntity",
                column: "InventoryEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryPerkEntity_PerkId",
                table: "InventoryPerkEntity",
                column: "PerkId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cooldowns");

            migrationBuilder.DropTable(
                name: "GamblingChannels");

            migrationBuilder.DropTable(
                name: "InventoryItemEntity");

            migrationBuilder.DropTable(
                name: "InventoryPerkEntity");

            migrationBuilder.DropTable(
                name: "SeedHashes");

            migrationBuilder.DropTable(
                name: "WordleEntries");

            migrationBuilder.DropTable(
                name: "ItemEntities");

            migrationBuilder.DropTable(
                name: "InventoryEntity");

            migrationBuilder.DropTable(
                name: "PerkEntities");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
