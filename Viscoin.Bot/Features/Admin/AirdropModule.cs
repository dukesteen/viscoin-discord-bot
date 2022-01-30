using Discord;
using Discord.Interactions;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Shared.Attributes;

namespace Viscoin.Bot.Features.Admin;

[Group("airdrop", "airdrop commands")]
[RequireAdmin]
public class AirdropModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserService _userService;

    public AirdropModule(UserService userService)
    {
        _userService = userService;
    }

    [SlashCommand("user", "airdrop naar een user")]
    public async Task AirdropUser(IUser target, int amount)
    {
        var user = await _userService.GetOrCreateUser(target);
        await _userService.AddCoinsAsync(user, amount);

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Airdropped")
            .WithDescription($"{amount} {AppConstants.ViscoinEmote} verstuurd naar {user.Nickname ?? user.Username}");

        await RespondAsync(embed: embedBuilder.Build());
    }
}