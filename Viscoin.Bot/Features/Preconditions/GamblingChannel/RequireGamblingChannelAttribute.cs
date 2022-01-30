using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.Preconditions.GamblingChannel;

public class RequireGamblingChannelAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var scope = services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        if (context.Channel is IThreadChannel)
            return PreconditionResult.FromSuccess();
        
        if (db.GamblingChannels.Any(x => x.ChannelId == context.Channel.Id))
        {
            await db.DisposeAsync();
            return PreconditionResult.FromSuccess();
        }

        return PreconditionResult.FromError("Je kan deze command niet uitvoeren in dit kanaal.");
    }
}