using Microsoft.Extensions.DependencyInjection;
using Viscoin.Bot.Features.Games.Blackjack;
using Viscoin.Bot.Features.Games.Dice;
using Viscoin.Bot.Features.Games.Mines;
using Viscoin.Bot.Features.Games.Woordle;

namespace Viscoin.Bot.Features.Games;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddGames(this IServiceCollection services)
    {
        return services
            .AddSingleton<DiceService>()
            .AddSingleton<MinesService>()
            .AddSingleton<BlackjackService>()
            .AddSingleton<WoordleService>();

    }
}