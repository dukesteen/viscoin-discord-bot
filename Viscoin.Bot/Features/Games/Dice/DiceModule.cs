using System.Text;
using Discord;
using Discord.Interactions;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Games.Dice;

public class DiceModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserService _userService;
    private readonly DiceService _diceService;

    public DiceModule(UserService userService, DiceService diceService)
    {
        _userService = userService;
        _diceService = diceService;
    }

    [SlashCommand("dice", "dice")]
    public async Task Dice(DicePicks choice, [MinValue(0.02)] [MaxValue(99.98)] double pick, [MinValue(1)] int amount, [MinValue(1)] [MaxValue(20)] int times = 1)
        {
            var user = await _userService.GetOrCreateUser(Context.User);

            if(user.Balance < amount * times)
            {
                await RespondAsync(embed: EmbedConstants.NotEnoughBalanceEmbed, ephemeral: true);
                return;
            }

            
            var serverSeed = user.ServerSeed.ToString();
            var clientSeed = user.ClientSeed;

            var responseStringBuilder = new StringBuilder();
            var total = 0;

            for(int i = 0; i < times; i++)
            {
                var dicePick = Math.Round(FairRandom.GetRandomFloats(serverSeed, clientSeed, user.Nonce, 1)[0] * 10001 / 100, 2);

                var multiplier = _diceService.GetMultiplier(choice, pick);

                var winnings = _diceService.GetWinnings(choice, dicePick, pick, multiplier, amount);

                total += winnings;

                if (winnings == 0)
                {
                    responseStringBuilder.AppendLine($"De uitkomst van deze bet is 0");
                }
                else if (winnings > 0)
                {
                    responseStringBuilder.AppendLine($"Rolled `{dicePick}`. Je hebt {winnings} {AppConstants.ViscoinEmote} gewonnen");
                }
                else
                {
                    responseStringBuilder.AppendLine($"Rolled `{dicePick}`. Je hebt {amount} {AppConstants.ViscoinEmote} verloren");
                }

                user = await _userService.IncreaseNonceAsync(user);
            }

            if(times > 1)
            {
                responseStringBuilder.AppendLine($"--------------");
                responseStringBuilder.AppendLine($"Totaal: {total} {AppConstants.ViscoinEmote}");
            }

            try
            {
                await _userService.AddCoinsAsync(user, total);
            } catch (Exception)
            {
                await RespondAsync("Er is iets mis gegaan");
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Dice")
                .WithDescription(responseStringBuilder.ToString());
            
            await RespondAsync(embed: embedBuilder.Build());
        }
}