using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viscoin.Bot.Features.User;

namespace Viscoin.Bot.Features.Lottery.Types;

public class LotteryEntry
{
    public ulong UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public int TicketAmount { get; set; }
    
    public int LotteryId { get; set; }
    public Lottery Lottery { get; set; } = null!;
}

public class LotteryEntryConfiguration : IEntityTypeConfiguration<LotteryEntry>
{
    public void Configure(EntityTypeBuilder<LotteryEntry> builder)
    {
        builder.HasKey(c => new { c.UserId, c.LotteryId });
    }
}