using Discord.Interactions;
using Discord.WebSocket;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;

namespace Viscoin.Bot.Features.Coindrops;

public class CoindropModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserService _userService;

    public CoindropModule(UserService userService)
    {
        _userService = userService;
    }

    [ComponentInteraction("claim-coins:*")]
    public async Task ClaimCoins(string arg1)
    {
        int amount = int.Parse(arg1);

        var user = await _userService.GetOrCreateUser(Context.User);
        await _userService.AddCoinsAsync(user, amount);

        await Context.Channel.SendMessageAsync($"{((SocketGuildUser)Context.User).Nickname ?? Context.User.Username} heeft {amount} {AppConstants.ViscoinEmote} geclaimed van een drop.");
        
        var originalResponse = (SocketMessageComponent)Context.Interaction;
        await Context.Channel.DeleteMessageAsync(originalResponse.Message.Id);
    }
}