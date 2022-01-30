using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viscoin.Bot.Features.User;

namespace Viscoin.Bot.Features.Statistics.Types;

public class CommandExecuted
{
    public CommandExecuted(string commandName, DateTime timeExecuted, ulong userId)
    {
        CommandName = commandName;
        TimeExecuted = timeExecuted;
        UserId = userId;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string CommandName { get; set; }
    
    [Column(TypeName = "timestamp")]
    public DateTime TimeExecuted { get; set; }
    
    public ulong UserId { get; set; }
    public UserEntity User { get; set; } = null!;
}

public class CommandExecutedConfiguration : IEntityTypeConfiguration<CommandExecuted>
{
    public void Configure(EntityTypeBuilder<CommandExecuted> builder)
    {
        builder.HasKey(x => new { x.Id, x.TimeExecuted });
    }
}