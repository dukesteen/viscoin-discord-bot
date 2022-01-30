namespace Viscoin.Bot.Infrastructure.Config;

public class BotConfig
{
    public string Token { get; init; } = null!;
    public string ApplicationDbContext { get; init; } = null!;
    public string HangfireDbContext { get; init; } = null!;
    public ulong TestGuild { get; init; }
    public ulong ViscordGuild { get; init; }
}