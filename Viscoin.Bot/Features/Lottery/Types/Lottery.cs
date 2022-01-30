using System.ComponentModel.DataAnnotations.Schema;

namespace Viscoin.Bot.Features.Lottery.Types;

public class Lottery
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int PrizePool { get; set; }
    public int TicketPrice { get; set; }
    public int MaxTickets { get; set; }
    public ulong LotteryMessageChannelId { get; set; }
    public ulong LotteryMessageId { get; set; }
    public bool IsActive { get; set; }
    public List<LotteryEntry> Entries { get; set; } = new();
}