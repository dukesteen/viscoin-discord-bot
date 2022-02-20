using System.Drawing;
using System.Globalization;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Humanizer;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;
using Viscoin.Bot.Features.Statistics.Types;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

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

    [SlashCommand("balstats", "chart statistics")]
    public async Task Stats(TimeSpan interval, IUser? user = null)
    {
        await DeferAsync();
        
        var finalUser = user ?? Context.User;

        if (finalUser is not SocketGuildUser guildUser)
            return;
            
        var data = await _statService.GetBalanceHistory(interval, user ?? Context.User);

        var values = data.Select(x => x.Balance).Reverse();
        
        var chart = new SKCartesianChart
        {
            Background = SKColor.Parse("#00171f"),
            Width = 1400,
            Height = 600,
            XAxes = new ICartesianAxis[]
            {
                new Axis()
                {
                    IsVisible = false
                }
            },
            Series = new ISeries[]
            {
                new LineSeries<int>
                {
                    Stroke = new SolidColorPaint(SKColor.Parse("#4361ee"), 4),
                    
                    GeometrySize = 7,
                    
                    Fill = new SolidColorPaint(SKColors.Transparent),
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsPosition = DataLabelsPosition.Bottom,
                    DataLabelsFormatter = point => point.Model.ToMetric(decimals: 2),
                    Values = values,
                }
            }
        };

        var url = await DiscordUtilities.UploadImageGetUrlAsync(Context,
            chart.GetImage().Encode(SKEncodedImageFormat.Png, 80).AsStream());
        
        var embed = new EmbedBuilder()
            .WithTitle($"Statistieken voor {guildUser.Nickname ?? guildUser.Username}")
            .WithDescription($"Deze statistieken lopen van {data.Last().Time.Humanize()} tot nu")
            .WithImageUrl(url)
            .Build();
        
        await FollowupAsync(embed: embed);
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