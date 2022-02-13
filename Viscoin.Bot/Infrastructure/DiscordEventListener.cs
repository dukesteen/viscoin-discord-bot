using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Viscoin.Bot.Infrastructure.Messages;

namespace Viscoin.Bot.Infrastructure;

public class DiscordEventListener
{
    private readonly CancellationToken _cancellationToken;

    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceScopeFactory _serviceScope;

    public DiscordEventListener(DiscordSocketClient client, IServiceScopeFactory serviceScope, InteractionService commands)
    {
        _client = client;
        _serviceScope = serviceScope;
        _commands = commands;
        _cancellationToken = new CancellationTokenSource().Token;
    }

    private IMediator Mediator
    {
        get
        {
            var scope = _serviceScope.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IMediator>();
        }
    }

    public Task StartAsync()
    {
        _client.Ready += OnReadyAsync;
        _client.MessageReceived += OnMessageReceivedAsync;
        _client.InteractionCreated += OnInteractionCreatedAsync;

        _commands.SlashCommandExecuted += OnSlashCommandExecutedAsync;

        return Task.CompletedTask;
    }

    private Task OnSlashCommandExecutedAsync(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        return Mediator.Publish(new SlashCommandExecutedNotification(arg1, arg2, arg3), _cancellationToken);
    }

    private Task OnInteractionCreatedAsync(SocketInteraction arg)
    {
        return Mediator.Publish(new InteractionCreatedNotification(arg), _cancellationToken);
    }

    private Task OnMessageReceivedAsync(SocketMessage arg)
    {
        return Mediator.Publish(new MessageReceivedNotification(arg), _cancellationToken);
    }

    private Task OnReadyAsync()
    {
        return Mediator.Publish(ReadyNotification.Default, _cancellationToken);
    }
}