namespace Viscoin.Bot.Features.Inventory.Types;

public class InventoryPerkEntity
{
    public InventoryPerkEntity(string perkId, string metadata = "")
    {
        PerkId = perkId;
        Metadata = metadata;
    }

    public Guid Id { get; set; }
    
    public Guid InventoryEntityId { get; set; }
    public InventoryEntity Inventory { get; set; } = null!;

    public string PerkId { get; set; }
    public PerkEntity Perk { get; set; } = null!;

    public string Metadata { get; set; }
}