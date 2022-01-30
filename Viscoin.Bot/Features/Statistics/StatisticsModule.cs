using System.Globalization;
using System.Text;
using Discord;
using Discord.Interactions;
using Humanizer;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;

namespace Viscoin.Bot.Features.Statistics;

public class StatisticsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly StatService _statService;
    private readonly UserService _userService;

    public StatisticsModule(StatService statService, UserService userService)
    {
        _statService = statService;
        _userService = userService;
    }

    [SlashCommand("richlist", "zie wie de rijksten zijn")]
    public async Task ShowRichlist()
    {
        var user = await _userService.GetOrCreateUser(Context.User);
        var allTopUsers = _statService.GetRichestUsers();
        var topUsers = allTopUsers.Take(10).ToList();
        var topUsersString = new StringBuilder();
        foreach (var it in topUsers.Select((x, i) => new { Value = x, Index = i }))
        {
            topUsersString.AppendLine($"#{it.Index + 1} <@{it.Value.Id}>: {it.Value.Balance:N0} {AppConstants.ViscoinEmote}");     
        }

        if (allTopUsers.FindIndex(x => x.Id == user.Id) > 10)
        {
            topUsersString.AppendLine("...");
            topUsersString.AppendLine($"#{allTopUsers.FindIndex(x => x.Id == user.Id) + 1} <@{user.Id}>: {user.Balance:N0} {AppConstants.ViscoinEmote}");  
        }
        
        var embedBuilder = new EmbedBuilder();

        embedBuilder.WithTitle("Rijkste mensen");
        embedBuilder.WithDescription(topUsersString.ToString());

        await RespondAsync(embed: embedBuilder.Build());
    }
}

[Group("top", "top statistics")]
public class TopGroup : InteractionModuleBase<SocketInteractionContext>
{
    private readonly StatService _stats;

    public TopGroup(StatService stats)
    {
        _stats = stats;
    }

    [SlashCommand("active", "meest actieve gebruikers in de gespecificeerde timespan")]
    public async Task TopActiveUsers(TimeSpan timespan)
    {
        var users = await _stats.GetMostActiveUsersAsync(timespan);
        var topUsers = users.Take(10).ToList();

        var builder = new StringBuilder();
        
        for (int i = 0; i < topUsers.Count; i++)
        {
            var info = topUsers[i];
            builder.AppendLine($"#{i + 1} <@{info.UserId}>: {info.TimesUsed} commands");
        }

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"Meest actieve gebruikers ({timespan.Humanize(culture: CultureInfo.GetCultureInfo("nl-NL"))})")
            .WithDescription(builder.ToString());

        await RespondAsync(embed: embedBuilder.Build());

    }
}