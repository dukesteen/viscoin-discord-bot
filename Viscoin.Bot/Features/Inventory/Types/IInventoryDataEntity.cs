namespace Viscoin.Bot.Features.Inventory.Types;

public interface IInventoryDataEntity
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Cost { get; set; }
    public int MaxQuantity { get; set; }
}