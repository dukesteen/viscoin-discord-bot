using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Viscoin.Bot.Features.Preconditions.Cooldown;

public class CooldownEntity
{
    public CooldownEntity(ulong userId, string commandName, DateTime lastTimeRan)
    {
        UserId = userId;
        CommandName = commandName;
        LastTimeRan = lastTimeRan;
    }

    public ulong UserId { get; set; }
    public string CommandName { get; set; }
    public DateTime LastTimeRan { get; set; }
}

public class CooldownEntityConfiguration : IEntityTypeConfiguration<CooldownEntity>
{
    public void Configure(EntityTypeBuilder<CooldownEntity> builder)
    {
        builder.HasKey(x => new { x.UserId, x.CommandName });
    }
}