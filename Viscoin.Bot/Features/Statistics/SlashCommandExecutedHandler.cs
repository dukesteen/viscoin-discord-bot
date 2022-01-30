using MediatR;
using Viscoin.Bot.Features.Statistics.Types;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Infrastructure.Data;
using Viscoin.Bot.Infrastructure.Messages;

namespace Viscoin.Bot.Features.Statistics;

public class SlashCommandExecutedHandler : INotificationHandler<SlashCommandExecutedNotification>
{
    private readonly UserService _userService;
    private readonly ApplicationDbContext _db;
    
    public SlashCommandExecutedHandler(UserService userService, ApplicationDbContext db)
    {
        _userService = userService;
        _db = db;
    }

    public async Task Handle(SlashCommandExecutedNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.Result.IsSuccess)
            return;
        
        _ = await _userService.GetOrCreateUser(notification.Context.User);

        _db.CommandsExecuted.Add(
            new CommandExecuted(notification.Info.Name, DateTime.Now, notification.Context.User.Id));

        await _db.SaveChangesAsync(cancellationToken);
    }
}