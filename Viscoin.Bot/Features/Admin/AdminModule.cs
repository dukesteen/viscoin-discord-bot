using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Caching.Memory;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Shared.Attributes;

namespace Viscoin.Bot.Features.Admin;

[Group("admin", "admin commands")]
[RequireAdmin]
public class AdminModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly AdminService _adminService;
    private readonly UserService _userService;

    public AdminModule(AdminService adminService, UserService userService)
    {
        _adminService = adminService;
        _userService = userService;
    }
    
    [SlashCommand("setgambling", "Zorg dat gambling commands uitgevoerd kunnen worden in een channel")]
    public async Task SetGambling(IGuildChannel? arg1 = null)
    {
        var channel = (arg1 ?? Context.Channel as IGuildChannel)!;
        var result = await _adminService.SetGambling(channel);

        if (result)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Success")
                .WithDescription($"`{channel.Name}` gemarkeerd als gok kanaal");
            
            await RespondAsync(embed: embedBuilder.Build());
        }
        else
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Error")
                .WithDescription($"`{channel.Name}` is al een gok kanaal");

            await RespondAsync(embed: embedBuilder.Build());
        }
    }

    [SlashCommand("void", "haal coins van iemand weg")]
    public async Task VoidCoins(IUser target, int amount)
    {
        var user = await _userService.GetOrCreateUser(target);
        await _userService.RemoveCoinsAsync(user, amount);

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Success!")
            .WithDescription($"{amount} {AppConstants.ViscoinEmote} weggehaald van {user.Nickname ?? user.Username}");

        await RespondAsync(embed: embedBuilder.Build());
    }
}

[Group("lock", "lock commands")]
[RequireAdmin]
public class LockGroup : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMemoryCache _cache;

    public LockGroup(IMemoryCache cache)
    {
        _cache = cache;
    }

    [SlashCommand("add", "lock een command")]
    public async Task AddLock(string commandName)
    {
        if (!_cache.TryGetValue(CacheKeys.LockedCommands, out List<string> lockedCommands))
        {
            _cache.Set(CacheKeys.LockedCommands, new List<string> { commandName });
            await RespondAsync($"`{commandName}` gelocked");
            return;
        }

        if (!lockedCommands.Contains(commandName))
        {
            lockedCommands.Add(commandName);
            _cache.Set(CacheKeys.LockedCommands, lockedCommands);

            await RespondAsync($"`{commandName}` gelocked");
            return;
        }

        await RespondAsync("Deze command is al gelocked");
    }
    
    [SlashCommand("remove", "unlock een command")]
    public async Task RemoveLock(string commandName)
    {
        if (!_cache.TryGetValue(CacheKeys.LockedCommands, out List<string> lockedCommands))
        {
            await RespondAsync($"`Er zijn geen commands gelocked");
            return;
        }

        if (lockedCommands.Contains(commandName))
        {
            lockedCommands.Remove(commandName);
            _cache.Set(CacheKeys.LockedCommands, lockedCommands);

            await RespondAsync($"`{commandName}` unlocked");
            return;
        }

        await RespondAsync("Deze command is niet gelocked");
    }

    [SlashCommand("list", "locked commands list")]
    public async Task LockList()
    {
        if (!_cache.TryGetValue(CacheKeys.LockedCommands, out List<string> lockedCommands))
        {
            await RespondAsync($"Er zijn geen commands gelocked");
            return;
        }

        await RespondAsync($"{String.Join(", ", lockedCommands)}");
    }
}