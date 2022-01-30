using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Viscoin.Bot.Features.Admin;
using Viscoin.Bot.Features.Economy;
using Viscoin.Bot.Features.Games;
using Viscoin.Bot.Features.Inventory;
using Viscoin.Bot.Features.Lottery;
using Viscoin.Bot.Features.Shop;
using Viscoin.Bot.Features.Statistics;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Infrastructure;
using Viscoin.Bot.Infrastructure.Commands;
using Viscoin.Bot.Infrastructure.Config;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot;

public class Bot
{
    private static readonly TimeSpan ResetTimeout = TimeSpan.FromSeconds(15);

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddMemoryCache()
            .AddMediatR(typeof(Bot))
            .AddLogging(l => l.AddSerilog())
            .AddDbContext<ApplicationDbContext>(ContextOptions)
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
                GatewayIntents = GatewayIntents.All,
                LogLevel = LogSeverity.Info
            }))
            .AddSingleton<DiscordEventListener>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionCommandHandler>()
            .AddScoped<UserService>()
            .AddScoped<AdminService>()
            .AddScoped<EconomyService>()
            .AddScoped<InventoryService>()
            .AddScoped<ShopService>()
            .AddScoped<StatService>()
            .AddScoped<LotteryService>()
            .AddSingleton<InteractiveService>()
            .AddSingleton<Random>()
            .AddGames()
            .BuildServiceProvider();
    }

    public static async Task Main()
    {
        await new Bot().RunAsync();
    }

    private async Task RunAsync()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Hangfire", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        await using var services = ConfigureServices();
        
        await services.GetRequiredService<InteractionCommandHandler>().InitializeAsync();

        var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<InventoryService>().InitializeAsync();
        scope.Dispose();

        var client = services.GetRequiredService<DiscordSocketClient>();
        var commands = services.GetRequiredService<InteractionService>();
        var listener = services.GetRequiredService<DiscordEventListener>();

        client.Log += LogAsync;
        client.Connected += ClientOnConnected;
        client.Disconnected += _ => ClientOnDisconnected(client);
        
        commands.Log += LogAsync;
        
        await listener.StartAsync();

        await client.LoginAsync(TokenType.Bot, ViscoinConfig.Configuration.Token);
        await client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }
    
    private static Task LogAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };

        Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);

        return Task.CompletedTask;
    }

    private static async Task CheckStateAsync(IDiscordClient client)
    {
        // Client reconnected, no need to reset
        if (client.ConnectionState == ConnectionState.Connected) return;

        Log.Information("Attempting to reset the client");

        var timeout = Task.Delay(ResetTimeout);
        var connect = client.StartAsync();
        var task = await Task.WhenAny(timeout, connect);

        if (task == timeout)
        {
            Log.Fatal("Client reset timed out (task deadlocked?), killing process");
            FailFast();
        }
        else if (connect.IsFaulted)
        {
            Log.Fatal(connect.Exception, "Client reset faulted, killing process");
            FailFast();
        }
        else if (connect.IsCompletedSuccessfully)
        {
            Log.Information("Client reset successfully!");
        }
    }

    private Task ClientOnConnected()
    {
        Log.Debug("Client reconnected, resetting cancel tokens...");

        Log.Debug("Client reconnected, cancel tokens reset");
        return Task.CompletedTask;
    }

    private Task ClientOnDisconnected(IDiscordClient client)
    {
        // Check the state after <timeout> to see if we reconnected
        Log.Information("Client disconnected, starting timeout task...");
        _ = Task.Delay(ResetTimeout).ContinueWith(async _ =>
        {
            Log.Debug("Timeout expired, continuing to check client state...");
            await CheckStateAsync(client);
            Log.Debug("State came back okay");
        });

        return Task.CompletedTask;
    }

    private static void ContextOptions(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(ViscoinConfig.Configuration.ApplicationDbContext);
    }

    private static void FailFast()
    {
        Environment.Exit(1);
    }
}