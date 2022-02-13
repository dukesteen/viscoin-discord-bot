using Discord;

namespace Viscoin.Bot.Shared;

public static class AppConstants
{
    public const string ViscoinEmote = "<:viscoin:925105595051294750>";
    public const ulong BotDepositChannel = 935322624844767233;

    public static readonly List<string> Ranks = new()
    {
        "A",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "10",
        "J",
        "V",
        "H"
    };

    public static readonly List<char> Suits = new()
    {
        '♥',
        '♦',
        '♠',
        '♣'
    };
}

public static class DrawingConstants
{
    public const int CardWidth = 250;
    public const int CardHeight = 350;
}

public static class EmbedConstants
{
    public static readonly Embed NotEnoughBalanceEmbed = new EmbedBuilder()
        .WithTitle("Error!")
        .WithDescription("Je hebt niet genoeg coins om dit te doen")
        .Build();
}

public static class CacheKeys
{
    public static object LockedCommands = new ();
    public static object DropKey = new();
    public static object ActiveWoordleUsers = new();
}