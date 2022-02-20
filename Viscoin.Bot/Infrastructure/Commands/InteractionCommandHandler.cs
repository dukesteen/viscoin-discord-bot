using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using Viscoin.Bot.Infrastructure.Config;
using Viscoin.Bot.Infrastructure.Messages;
using Viscoin.Bot.Infrastructure.TypeConverters;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Infrastructure.Commands;

public class InteractionCommandHandler : INotificationHandler<InteractionCreatedNotification>, INotificationHandler<ReadyNotification>, INotificationHandler<SlashCommandExecutedNotification>
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private readonly IMemoryCache _cache;

    public InteractionCommandHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services, IMemoryCache cache)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _cache = cache;
    }

    public async Task Handle(InteractionCreatedNotification notification, CancellationToken cancellationToken)
    {
        var interaction = notification.Interaction;

        try
        {
            if (interaction is IApplicationCommandInteraction command)
                if (_cache.TryGetValue(CacheKeys.LockedCommands, out List<string> lockedCommands))
                    if (lockedCommands.Contains(command.Data.Name))
                    {
                        await interaction.RespondAsync(
                            embed: EmbedUtilities.CreateErrorEmbed("Deze command is op het moment gelocked."));
                        return;
                    }

            var ctx = new SocketInteractionContext(_client, interaction);
            await _commands.ExecuteCommandAsync(ctx, _services);
            Log.Debug("Received interaction from {Username}", interaction.User.Username);
        }
        catch (Exception)
        {
            Log.Warning("Interaction failed for user with id {UserId}", interaction.User.Id);

            if (interaction.Type == InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async msg => await msg.Result.DeleteAsync(), cancellationToken);
        }
    }
    
    public Task Handle(SlashCommandExecutedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.Result.IsSuccess)
            return Task.CompletedTask;

        EmbedBuilder embedBuilder;
        
        switch (notification.Result.Error)
        {
            case InteractionCommandError.ConvertFailed:
                embedBuilder = new EmbedBuilder()
                    .WithTitle("Error!")
                    .WithDescription("Deze parameter kon niet geconverteerd worden.");
                break;
            default:
                embedBuilder = new EmbedBuilder()
                    .WithTitle("Error!")
                    .WithDescription(notification.Result.ErrorReason);
                break;
        }
        
        notification.Context.Interaction.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
        
        return Task.CompletedTask;
    }

    public async Task Handle(ReadyNotification notification, CancellationToken cancellationToken)
    {
#if DEBUG
        await _commands.RegisterCommandsToGuildAsync(ViscoinConfig.Configuration.TestGuild);
#else
        await _commands.RegisterCommandsToGuildAsync(ViscoinConfig.Configuration.ViscordGuild);
#endif
    }

    public async Task InitializeAsync()
    {
        _commands.AddTypeConverter<TimeSpan>(new TimespanTypeConverter());
        
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }
}