using MediatR;

namespace Viscoin.Bot.Infrastructure.Messages;

public class ReadyNotification : INotification
{
    public static readonly ReadyNotification Default
        = new();

    private ReadyNotification()
    {
    }
}