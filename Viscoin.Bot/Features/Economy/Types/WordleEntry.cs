using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Viscoin.Bot.Features.Economy.Types;

public class WordleEntry
{
    public WordleEntry(ulong discordId, int wordleId)
    {
        DiscordId = discordId;
        WordleId = wordleId;
    }
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong DiscordId { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int WordleId { get; set; }
}

public class WordleEntryConfiguration : IEntityTypeConfiguration<WordleEntry>
{
    public void Configure(EntityTypeBuilder<WordleEntry> builder)
    {
        builder.HasKey(c => new
        {
            c.DiscordId, c.WordleId
        });
    }
}