using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viscoin.Bot.Features.User;

namespace Viscoin.Bot.Features.Statistics.Types;

public class BalanceUpdate
{
    public BalanceUpdate(int resultingBalance, int mutation, DateTime timestamp, ulong userId)
    {
        ResultingBalance = resultingBalance;
        Mutation = mutation;
        Timestamp = timestamp;
        UserId = userId;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public int ResultingBalance { get; set; }
    public int Mutation { get; set; }
    
    [Column(TypeName = "timestamp")]
    public DateTime Timestamp { get; set; }
    
    public ulong UserId { get; set; }
    public UserEntity User { get; set; } = null!;
}

public class BalanceUpdateConfiguration : IEntityTypeConfiguration<BalanceUpdate>
{
    public void Configure(EntityTypeBuilder<BalanceUpdate> builder)
    {
        builder.HasKey(c => new { c.Id, c.Timestamp });
    }
}