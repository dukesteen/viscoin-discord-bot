using Discord;

namespace Viscoin.Bot.Utilities;

public class EmbedUtilities
{
    public static Embed CreateErrorEmbed(string message)
    {
        return new EmbedBuilder()
            .WithTitle("Error!")
            .WithDescription(message)
            .Build();
    }

    public static Embed CreateEmbedWithTitle(string title, string message)
    {
        return new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(message)
            .Build();
    }
}