using Discord;
using Discord.Interactions;
using MediatR;

namespace Viscoin.Bot.Infrastructure.Messages;

public class SlashCommandExecutedNotification : INotification
{
    public SlashCommandExecutedNotification(SlashCommandInfo info, IInteractionContext context, IResult result)
    {
        Info = info;
        Context = context;
        Result = result;
    }

    public SlashCommandInfo Info { get; }
    public IInteractionContext Context { get; }
    public IResult Result { get; }
}