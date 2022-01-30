namespace Viscoin.Bot.Features.Inventory.Types;

public class InventoryItemEntity
{
    public InventoryItemEntity(string itemId, string metadata)
    {
        ItemId = itemId;
        Metadata = metadata;
    }

    public Guid Id { get; set; }
    
    public Guid InventoryEntityId { get; set; }
    public InventoryEntity Inventory { get; set; } = null!;

    public string ItemId { get; set; }
    public ItemEntity Item { get; set; } = null!;

    public string Metadata { get; set; }
}