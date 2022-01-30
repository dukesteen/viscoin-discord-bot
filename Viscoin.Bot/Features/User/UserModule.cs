using System.Text;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.User;

public class UserModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserService _userService;
    private readonly InteractiveService _interactiveService;

    public UserModule(UserService userService, InteractiveService interactiveService)
    {
        _userService = userService;
        _interactiveService = interactiveService;
    }
    
    [SlashCommand("balance", "Zie de balance van jezelf of iemand anders")]
    public async Task Balance(IUser? target = null)
    {
        await DeferAsync();
        
        var user = await _userService.GetOrCreateUser(target ?? Context.User);

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"Balance van {user.Nickname ?? user.Username}")
            .WithDescription($"{user.Balance:N0} {AppConstants.ViscoinEmote}");

        await FollowupAsync(embed: embedBuilder.Build());
    }
    
    [SlashCommand("pay", "stuur coins naar iemand anders")]
    public async Task Pay(IUser to, int amount)
    {
        var fromUser = await _userService.GetOrCreateUser(Context.User);
        var toUser = await _userService.GetOrCreateUser(to);

        if (fromUser.Balance < amount)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Je hebt niet genoeg coins om dit te doen"));
            return;
        }

        await _userService.RemoveCoinsAsync(fromUser, amount);
        await _userService.AddCoinsAsync(toUser, amount);

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Payment")
            .WithDescription(
                $"{amount} {AppConstants.ViscoinEmote} verstuurd naar {toUser.Nickname ?? toUser.Username}");

        await RespondAsync(embed: embedBuilder.Build());
    }

    [SlashCommand("profile", "profiel")]
    public async Task ViewProfile(IUser? target = null)
    {
        var user = await _userService.GetOrCreateUser(target ?? Context.User, x => x
            .Include(userEntity => userEntity.Inventory).ThenInclude(inventoryEntity => inventoryEntity.Perks).ThenInclude(inventoryPerkEntity => inventoryPerkEntity.Perk).AsSplitQuery().AsNoTracking()
            .Include(userEntity => userEntity.Inventory).ThenInclude(inventoryEntity => inventoryEntity.Items).ThenInclude(inventoryItemEntity => inventoryItemEntity.Item).AsSplitQuery().AsNoTracking());

        var pages = new List<PageBuilder>();

        var discordUser = target ?? await Context.Client.GetUserAsync(user.Id);

        var firstPage = new PageBuilder()
            .WithTitle("Profiel")
            .WithThumbnailUrl(discordUser.GetAvatarUrl())
            .AddField("Balance", $"{user.Balance} {AppConstants.ViscoinEmote}");
        
        pages.Add(firstPage);

        if (user.Inventory.Perks.Count > 0)
        {
            var perksPage = new PageBuilder()
                .WithTitle("Perks");

            var perksPageStringBuilder = new StringBuilder();
            int totalPerkValue = 0;
        
            foreach (var perk in user.Inventory.Perks.Select(x => x.PerkId).Distinct())
            {
                var perkCount = user.Inventory.Perks.Count(x => x.PerkId == perk);
                var perkInfo = user.Inventory.Perks.First(x => x.PerkId == perk).Perk;
                perksPageStringBuilder.AppendLine($"`{perkCount}/{perkInfo.MaxQuantity}` {perkInfo.Title}");
                totalPerkValue += perkCount * perkInfo.Cost;
            }

            perksPageStringBuilder.AppendLine($"\nWaarde: {totalPerkValue} {AppConstants.ViscoinEmote}");

            perksPage.WithDescription(perksPageStringBuilder.ToString());
        
            pages.Add(perksPage);
        }

        var paginator = new StaticPaginatorBuilder()
            {
                ActionOnCancellation = ActionOnStop.DeleteMessage
            }
            .AddUser(Context.User)
            .WithPages(pages)
            .Build();
        
        await _interactiveService.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));
    }
    
    [SlashCommand("modifycolor", "pas de kleur aan van je custom role")]
    public async Task ModifyColor(
        [MinValue(0)] [MaxValue(255)] int red,
        [MinValue(0)] [MaxValue(255)] int green,
        [MinValue(0)] [MaxValue(255)] int blue)
    {
        var userWithPerks = await _userService.GetOrCreateUser(Context.User,
            x => x
                .Include(userEntity => userEntity.Inventory).ThenInclude(inventoryEntity => inventoryEntity.Perks));

        var perk = userWithPerks.Inventory.Perks.FirstOrDefault(x => x.PerkId == "CUSTOM_COLOR");
        if (perk == null)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Je hebt geen color role gekocht"));
            return;
        }

        var roleId = (ulong)JsonConvert.DeserializeObject<dynamic>(perk.Metadata)?["RoleId"]!;

        await Context.Guild.GetRole(roleId).ModifyAsync(x => x.Color = new Color(red, green, blue));

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Nieuwe kleur")
            .WithDescription($"Je kleur is veranderd naar ({red}, {green}, {blue})");

        await RespondAsync(embed: embedBuilder.Build());
    }
}
