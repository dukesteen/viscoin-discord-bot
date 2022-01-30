using System.Globalization;
using Discord;
using Discord.Interactions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.Preconditions.Cooldown;

public class CooldownAttribute : PreconditionAttribute
{
    private readonly TimeSpan _timeSpan;

    public CooldownAttribute(int days = 0, int hours = 0, int minutes = 0, int seconds = 0) 
        => _timeSpan = TimeSpan.Zero
            .Add(TimeSpan.FromDays(days))
            .Add(TimeSpan.FromHours(hours))
            .Add(TimeSpan.FromMinutes(minutes))
            .Add(TimeSpan.FromSeconds(seconds));
    
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var scope = services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cooldown = await db.Cooldowns.FirstOrDefaultAsync(x => x.CommandName == commandInfo.Name && x.UserId == context.User.Id);

        if (cooldown == null)
        {
            await db.Cooldowns.AddAsync(new CooldownEntity(context.User.Id, commandInfo.Name, DateTime.UtcNow));
            await db.SaveChangesAsync();
            await db.DisposeAsync();
            return PreconditionResult.FromSuccess();
        }

        if (cooldown.LastTimeRan.Add(_timeSpan) <= DateTime.UtcNow)
        {
            cooldown.LastTimeRan = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return PreconditionResult.FromSuccess();
        }

        var timespanRemaining = cooldown.LastTimeRan.Add(_timeSpan) - DateTime.UtcNow;
        
        return PreconditionResult.FromError($"Wacht {timespanRemaining.Humanize(culture: CultureInfo.CreateSpecificCulture("nl-NL"))}");
    }
}