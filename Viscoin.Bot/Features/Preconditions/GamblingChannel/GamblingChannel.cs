using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viscoin.Bot.Features.Preconditions.GamblingChannel;

public class GamblingChannel
{
    public GamblingChannel(ulong channelId)
    {
        ChannelId = channelId;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public ulong ChannelId { get; set; }
}