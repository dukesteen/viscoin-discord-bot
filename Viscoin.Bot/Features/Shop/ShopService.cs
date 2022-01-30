using Microsoft.EntityFrameworkCore;
using Viscoin.Bot.Features.Inventory.Types;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.Shop;

public class ShopService
{
    private readonly ApplicationDbContext _db;

    public ShopService(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<PerkEntity> GetAllPerks()
    {
        return _db.PerkEntities.AsNoTracking().ToList();
    }
}