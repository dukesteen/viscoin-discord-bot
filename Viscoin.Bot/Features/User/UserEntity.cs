using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viscoin.Bot.Features.Inventory.Types;
using Viscoin.Bot.Features.Statistics.Types;

namespace Viscoin.Bot.Features.User;

[Table("Users")]
public class UserEntity
{
    public UserEntity(ulong id, int balance = 0)
    {
        Id = id;
        Balance = balance;
        ServerSeed = Guid.NewGuid();
        NextServerSeed = Guid.NewGuid();
        ClientSeed = "Viscord";
        Inventory = new InventoryEntity(id);
    }
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }
    
    public int Balance { get; set; }
    
    public Guid ServerSeed { get; set; }
    
    public Guid NextServerSeed { get; set; }
    
    [Column(TypeName = "varchar(50)")]
    public string ClientSeed { get; set; }
    public int Nonce { get; set; }

    public InventoryEntity Inventory { get; set; }
    
    public List<CommandExecuted> CommandsExecuted { get; set; } = null!;

    [NotMapped] public string Username { get; set; } = null!;
    [NotMapped] public string? Nickname { get; set; }
}

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
        => builder.HasKey(x => x.Id);
}