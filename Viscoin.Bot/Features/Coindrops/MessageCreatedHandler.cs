using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Viscoin.Bot.Infrastructure.Messages;
using Viscoin.Bot.Shared;

namespace Viscoin.Bot.Features.Coindrops;

public class MessageReceivedHandler : INotificationHandler<MessageReceivedNotification>
{
    private readonly Random _random;
    private readonly IMemoryCache _cache;
    private readonly DiscordSocketClient _client;

    public MessageReceivedHandler(Random random, IMemoryCache cache, DiscordSocketClient client)
    {
        _random = random;
        _cache = cache;
        _client = client;
    }

    public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        var message = notification.Message;

        if (message.Author.IsBot)
            return;

        if (_random.Next(5) != 1)
            return;

        if (!_cache.TryGetValue(CacheKeys.DropKey, out DateTime lastDropSent))
        {
            _cache.Set(CacheKeys.DropKey, DateTime.Now);
            
            lastDropSent = DateTime.Parse("01-01-2001");
        }
        
        if (lastDropSent.Add(TimeSpan.FromMinutes(5)) > DateTime.Now)
            return;

        if (message.Channel is IThreadChannel)
            return;

        if (message.Channel.Id == 748661612180930620)
            return;

        ITextChannel channel = (ITextChannel)await _client.GetChannelAsync(message.Channel.Id);
        if (channel == null)
            return;
        
        var randomNum = _random.NextDouble();

        EmbedBuilder embedBuilder;
        int coinAmount;

        if (randomNum < 0.01d)
        {
            coinAmount = _random.Next(2000, 8000);
            embedBuilder = new EmbedBuilder()
                .WithTitle($"Legendary drop")
                .WithDescription($"{coinAmount} viscoin appeared")
                .WithColor(Color.Gold);
        } else if (randomNum < 0.1d)
        {
            coinAmount = _random.Next(300, 1000);
            embedBuilder = new EmbedBuilder()
                .WithTitle($"Epic drop")
                .WithDescription($"{coinAmount} viscoin appeared")
                .WithColor(Color.DarkPurple);
        }
        else
        {
            coinAmount = _random.Next(50, 300);
            embedBuilder = new EmbedBuilder()
                .WithTitle($"Common drop")
                .WithDescription($"{coinAmount} viscoin appeared")
                .WithColor(Color.LightGrey);
        }
            
        ComponentBuilder componentBuilder = new ComponentBuilder()
            .WithButton("Claim", $"claim-coins:{coinAmount}", emote: Emote.Parse(AppConstants.ViscoinEmote) , row: 0);

        _cache.Set(CacheKeys.DropKey, DateTime.Now);
        
        await channel.SendMessageAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());
    }
}