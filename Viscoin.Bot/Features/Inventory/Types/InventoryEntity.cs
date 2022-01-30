using Viscoin.Bot.Features.User;

namespace Viscoin.Bot.Features.Inventory.Types;

public class InventoryEntity
{
    public InventoryEntity(ulong userEntityId)
    {
        UserEntityId = userEntityId;
    }

    public Guid Id { get; set; }
    
    public ulong UserEntityId { get; set; }
    public UserEntity User { get; set; } = null!;

    public List<InventoryItemEntity> Items { get; set; } = new();
    public List<InventoryPerkEntity> Perks { get; set; } = new();
}