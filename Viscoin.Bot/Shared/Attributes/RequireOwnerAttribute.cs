using Discord;
using Discord.Interactions;

namespace Viscoin.Bot.Shared.Attributes;

public class RequireAdminAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        switch (context.Client.TokenType)
        {
            case TokenType.Bot:
                var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

                if (context.User.Id == application.Owner.Id || context.User.Id == 288421015866966017)
                    return PreconditionResult.FromSuccess();
                return PreconditionResult.FromError(ErrorMessage ?? "Command kan alleen gebruikt worden door admins");
            default:
                return PreconditionResult.FromError($"{nameof(RequireOwnerAttribute)} is not supported by this {nameof(TokenType)}.");
        }
    }
}
    
public class RequireOwnerAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        switch (context.Client.TokenType)
        {
            case TokenType.Bot:
                var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

                if (context.User.Id == application.Owner.Id)
                    return PreconditionResult.FromSuccess();
                return PreconditionResult.FromError(ErrorMessage ?? "Command kan alleen gebruikt worden door admins");
            default:
                return PreconditionResult.FromError($"{nameof(RequireOwnerAttribute)} is not supported by this {nameof(TokenType)}.");
        }
    }
}