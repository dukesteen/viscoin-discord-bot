using Discord.WebSocket;
using MediatR;

namespace Viscoin.Bot.Infrastructure.Messages;

public class InteractionCreatedNotification : INotification
{
    public InteractionCreatedNotification(SocketInteraction interaction)
    {
        Interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
    }

    public SocketInteraction Interaction { get; }
}