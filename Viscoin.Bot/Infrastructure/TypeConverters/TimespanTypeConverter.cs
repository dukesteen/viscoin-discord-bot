using Discord;
using Discord.Interactions;
using TimeSpanParserUtil;

namespace Viscoin.Bot.Infrastructure.TypeConverters;

public class TimespanTypeConverter : TypeConverter<TimeSpan>
{
    public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;

    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        if (TimeSpanParser.TryParse((string)option.Value, out var timeSpan))
        {
            return Task.FromResult(TypeConverterResult.FromSuccess(timeSpan));
        }
        
        return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Dit is geen valide timespan."));
    }
}