using Discord;
using Discord.Interactions;
using Serilog;
using Viscoin.Bot.Features.Preconditions.GamblingChannel;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Games.Coinflip;

public class CoinflipModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserService _userService;

    public CoinflipModule(UserService userService)
    {
        _userService = userService;
    }

    [SlashCommand("coinflip", "hopen dat ie op jouw keuze valt")]
    [RequireGamblingChannel]
    public async Task Coinflip(CoinflipChoices choice, [MinValue(2)] int amount)
    {
        var user = await _userService.GetOrCreateUser(Context.User);

        if (user.Balance < amount)
        {
            await RespondAsync(embed: EmbedConstants.NotEnoughBalanceEmbed, ephemeral: true);
            return;
        }

        var randomNumber = FairRandom.GetRandomFloats(user.ServerSeed.ToString(), user.ClientSeed, user.Nonce, 1)[0];

        await _userService.IncreaseNonceAsync(user);
        
        Log.Debug("Generated number: {GeneratedNumber}", randomNumber);
        
        EmbedBuilder embedBuilder;
        string imageUrl;
        
        if (randomNumber > 0.5)
        {
            imageUrl =
                "https://media.discordapp.net/attachments/703147632696098936/933452222736511046/kop.gif?width=567&height=567";
            
            embedBuilder = new EmbedBuilder()
                .WithImageUrl(
                    imageUrl);

            await RespondAsync(embed: embedBuilder.Build());

            await Task.Delay(TimeSpan.FromSeconds(1.7));

            if (choice == CoinflipChoices.Kop)
            {
                var winAmount = (int)(amount * 1.99) - amount;

                embedBuilder = new EmbedBuilder()
                    .WithTitle("Gewonnen")
                    .WithDescription(
                        $"Je hebt de goede keuze gemaakt en {winAmount} {AppConstants.ViscoinEmote} gewonnen");

                if (winAmount >= 30000)
                {
                    embedBuilder.WithImageUrl(
                        "https://media.discordapp.net/attachments/703147632696098936/935257252351332372/viscord_big_profit_GIF_1_1_1_1.gif?width=400&height=400");
                }
                
                await _userService.AddCoinsAsync(user, winAmount);
                
                await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embedBuilder.Build());

                return;
            }
        }
        else
        {
            imageUrl =
                "https://media.discordapp.net/attachments/703147632696098936/933452359932215346/munt.gif?width=567&height=567";
            
            embedBuilder = new EmbedBuilder()
                .WithImageUrl(
                    imageUrl);

            await RespondAsync(embed: embedBuilder.Build());

            await Task.Delay(TimeSpan.FromSeconds(1.7));

            if (choice == CoinflipChoices.Munt)
            {
                var winAmount = (int)(amount * 1.99) - amount;

                embedBuilder = new EmbedBuilder()
                    .WithTitle("Gewonnen")
                    .WithDescription(
                        $"Je hebt de goede keuze gemaakt en {winAmount} {AppConstants.ViscoinEmote} gewonnen");

                if (winAmount >= 30000)
                {
                    embedBuilder.WithImageUrl(
                        "https://media.discordapp.net/attachments/703147632696098936/935257252351332372/viscord_big_profit_GIF_1_1_1_1.gif?width=400&height=400");
                }
                
                await _userService.AddCoinsAsync(user, winAmount);
                
                await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embedBuilder.Build());

                return;
            }
        }

        embedBuilder = new EmbedBuilder()
            .WithTitle("Verloren")
            .WithDescription($"Je hebt de verkeerde keuze gemaakt en {amount} {AppConstants.ViscoinEmote} verloren");

        if (amount >= 50000)
        {
            embedBuilder.WithImageUrl(
                "https://media.discordapp.net/attachments/926246645199409223/934819988974469130/emotional-damage.gif?width=448&height=252");
        }
        
        await _userService.RemoveCoinsAsync(user, amount);

        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embedBuilder.Build());
    }
}