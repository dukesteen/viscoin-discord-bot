using System.ComponentModel.DataAnnotations.Schema;

namespace Viscoin.Bot.Features.Inventory.Types;

public class PerkEntity : IInventoryDataEntity
{
    public PerkEntity(string id, string title, string description, int cost, int maxQuantity)
    {
        Id = id;
        Title = title;
        Description = description;
        Cost = cost;
        MaxQuantity = maxQuantity;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column(TypeName = "varchar(50)")]
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Cost { get; set; }
    public int MaxQuantity { get; set; }
    
}