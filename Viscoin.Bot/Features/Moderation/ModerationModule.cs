using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Shared.Attributes;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Moderation;

public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserService _userService;

    public ModerationModule(UserService userService)
    {
        _userService = userService;
    }

    [SlashCommand("muteall", "mute iedereen in de voice channel")]
    [RequireAdmin]
    public async Task MuteAll()
    {
        var vc = Context.Guild.Users.Single(x => x.Id == Context.User.Id).VoiceChannel;
        
        if (vc == null)
        {
            await RespondAsync("Je moet in een voice kanaal zitten om iedereen te kunnen muten");
            return;
        }

        foreach (var user in vc.Users)
        {
            await user.ModifyAsync(x => x.Mute = true);
        }
        
        await RespondAsync("Iedereen gemute");
    }
    
    [SlashCommand("unmuteall", "unmute iedereen in de voice channel")]
    [RequireAdmin]
    public async Task UnmuteAll()
    {
        var vc = Context.Guild.Users.Single(x => x.Id == Context.User.Id).VoiceChannel;
        
        if (vc == null)
        {
            await RespondAsync("Je moet in een voice kanaal zitten om iedereen te kunnen unmuten");
            return;
        }

        foreach (var user in vc.Users)
        {
            await user.ModifyAsync(x => x.Mute = false);
        }
        
        await RespondAsync("Iedereen geunmute");
    }

    [SlashCommand("mute", "mute bas")]
    public async Task MuteBas()
    {
        if (Context.Guild.Users.Single(x => x.Id == 560386149475024897).VoiceChannel == null)
        {
            await RespondAsync("Bas zit niet in een voice kanaal");
        }
        else if (Context.Guild.Users.Single(x => x.Id == Context.User.Id).VoiceChannel == null)
        {
            await RespondAsync("Je moet in een voice kanaal zitten om bas te kunnen muten");
        }
        else
        {
            await Context.Guild.GetUser(560386149475024897).ModifyAsync(x => x.Mute = true);
            await RespondAsync("Bas gemute");
        }
    }

    [SlashCommand("unmute", "unmute bas")]
    public async Task UnmuteBas()
    {
        IUser? user = await Context.Channel.GetUserAsync(560386149475024897) as IGuildUser;

        if (user == null)
        {
            await RespondAsync("Kon bas niet vinden in dit kanaal");
            return;
        }
        
        var botUser = await _userService.GetOrCreateUser(user);
        
        if (Context.Guild.Users.Single(x => x.Id == 560386149475024897).VoiceChannel == null)
        {
            await RespondAsync("Bas zit niet in een voice kanaal");
        }
        else
        {
            if (Context.User.Id == 560386149475024897)
            {
                await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed($"Het kost {botUser.Balance + 1} {AppConstants.ViscoinEmote} om jezelf te unmuten"));
                return;
            }
            
            SocketGuildUser? guildUser = await Context.Channel.GetUserAsync(560386149475024897) as SocketGuildUser;
            guildUser?.ModifyAsync(x => x.Mute = false);

            await RespondAsync("Bas geunmute");
        }
    }
}