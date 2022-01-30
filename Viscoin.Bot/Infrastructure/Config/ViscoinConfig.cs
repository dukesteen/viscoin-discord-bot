using Microsoft.Extensions.Configuration;
using Viscoin.Bot.Features.Inventory.Types;

namespace Viscoin.Bot.Infrastructure.Config;

public class ViscoinConfig
{
    private static readonly IConfigurationRoot Secrets
        = new ConfigurationBuilder().AddUserSecrets<ViscoinConfig>(optional: true).Build();

    private static readonly IConfigurationRoot AppSettings
        = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)!.FullName)
            .AddJsonFile("appsettings.json", false)
            .Build();
    
    public BotConfig Debug { get; init; } = null!;

    public BotConfig Release { get; init; } = null!;

    public static BotConfig Configuration { get; } =
#if DEBUG
        Secrets.GetSection(nameof(Debug)).Get<BotConfig>();
#else
        Secrets.GetSection(nameof(Release)).Get<BotConfig>();
#endif

    public static List<ItemEntity> Items { get; } = AppSettings.GetSection("Items").Get<List<ItemEntity>>();
    public static List<PerkEntity> Perks { get; } = AppSettings.GetSection("Perks").Get<List<PerkEntity>>();
}