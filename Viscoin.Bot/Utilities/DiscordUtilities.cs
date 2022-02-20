using Discord.Interactions;
using Discord.WebSocket;
using Viscoin.Bot.Shared;

namespace Viscoin.Bot.Utilities;

public static class DiscordUtilities
{
    public static async Task<string> UploadImageGetUrlAsync(SocketInteractionContext ctx, Stream imageAsStream)
    {
        var channel = await ctx.Client.GetChannelAsync(AppConstants.BotDepositChannel) as ISocketMessageChannel;
        var message = await channel?.SendFileAsync(imageAsStream, $"{Guid.NewGuid():N}.png")!;
        return message.Attachments.First().Url;
    }
}