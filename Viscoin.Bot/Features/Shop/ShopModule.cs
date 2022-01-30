using System.Text;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using Viscoin.Bot.Features.Inventory.Types;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Shop;

[Group("shop", "shop commands")]
public class ShopModule : InteractionModuleBase<SocketInteractionContext>
{
    [Group("perks", "perk commands")]
    public class PerkGroup : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ShopService _shopService;
        private readonly UserService _userService;

        public PerkGroup(ShopService shopService, UserService userService)
        {
            _shopService = shopService;
            _userService = userService;
        }

        [SlashCommand("list", "Lijst van alle perks")]
        public async Task ListPerks(int page = 1)
        {
            var arrayPage = page - 1;
            var allPerks = _shopService.GetAllPerks();
            if (Math.Ceiling(allPerks.Count / 5d) < page)
            {
                await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Deze pagina is niet beschikbaar"));
                return;
            }
            
            var perks = allPerks.Chunk(5).ToList()[arrayPage];
            
            var builder = new StringBuilder();
            var components = new ComponentBuilder();

            var menu = new SelectMenuBuilder()
                .WithCustomId($"shop-items-menu")
                .WithPlaceholder("Selecteer een optie")
                .WithMaxValues(perks.Length);

            foreach (var perk in perks)
            {
                builder.AppendLine($"**{perk.Title}**: {perk.Cost:N0} {AppConstants.ViscoinEmote} \n{perk.Description}\n");
                menu.AddOption($"{perk.Title}", perk.Id, $"{perk.Cost}");
            }

            components.WithSelectMenu(menu);
            
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Perk shop")
                .WithDescription(builder.ToString())
                .WithFooter($"Pagina {page} van {(int)Math.Ceiling((double)(allPerks.Count / 5)) + 1}");

            

            await RespondAsync(embed: embedBuilder.Build(), components: components.Build());
        }

        [ComponentInteraction("shop-items-menu", true)]
        public async Task BuyPerk(string[] selectedItems)
        {
            await DeferAsync();
            var allPerks = _shopService.GetAllPerks();
            
            Log.Debug("Selected items: {SelectedItems}", selectedItems);

            var user = await _userService.GetOrCreateUser(Context.User,
                x => x.Include(userEntity => userEntity.Inventory)
                    .ThenInclude(inventoryEntity => inventoryEntity.Perks));

            var builder = new StringBuilder();

            var balance = user.Balance;
            var totalPurchaseCost = 0;
            List<InventoryPerkEntity> perks = new();
            
            foreach (var perkId in selectedItems)
            {
                var itemData = allPerks.First(x => x.Id == perkId);
                var userPerkQty = user.Inventory.Perks.Count(x => x.PerkId == perkId);

                string metadata = "";
                
                if (balance < itemData.Cost)
                {
                    builder.AppendLine($"Je hebt niet genoeg coins om {itemData.Title} te kopen");
                    continue;
                }
                
                if (userPerkQty + 1 > itemData.MaxQuantity)
                {
                    builder.AppendLine($"Je kan niet meer dan {itemData.MaxQuantity} {itemData.Title} kopen");
                    continue;
                }

                if (perkId == "CUSTOM_COLOR")
                {
                    var roleId = await ConfigureRole(Context, user.Username);
                    metadata = JsonConvert.SerializeObject(new
                    {
                        RoleId = roleId
                    });
                }
                
                perks.Add(new InventoryPerkEntity(perkId, metadata));

                totalPurchaseCost += itemData.Cost;
                balance -= itemData.Cost;
                builder.AppendLine($"{itemData.Title} gekocht voor {itemData.Cost} {AppConstants.ViscoinEmote}");
            }

            user = await _userService.RemoveCoinsAsync(user, totalPurchaseCost);
            await _userService.AddPerksAsync(user, perks);

            builder.AppendLine("+---------------+");
            builder.AppendLine($"Totaal: {totalPurchaseCost} {AppConstants.ViscoinEmote}");
            
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Rekening")
                .WithDescription(builder.ToString());
            
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Content = "⠀" );

            await FollowupAsync(embed: embedBuilder.Build(), ephemeral: true);
        }

        private async Task<ulong> ConfigureRole(SocketInteractionContext ctx, string roleName)
        {
            var role = await ctx.Guild.CreateRoleAsync(roleName, isMentionable: false);
            var botRole = Context.Guild.Roles.Single(x => x.Name is "Visbank" or "Vistest");
            
            await Context.Guild.ReorderRolesAsync(new[]
            {
                new ReorderRoleProperties(role.Id, botRole.Position - 1)
            });
            
            await ctx.Guild.Users.First(x => x.Id == ctx.User.Id).AddRoleAsync(role);
            return role.Id;
        }
    }
}