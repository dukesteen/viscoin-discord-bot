using System.Diagnostics;
using Discord;
using Microsoft.EntityFrameworkCore;
using Viscoin.Bot.Features.Inventory.Types;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Infrastructure.Config;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.Inventory;

public class InventoryService
{
    private readonly ApplicationDbContext _db;
    private readonly UserService _userService;

    public InventoryService(ApplicationDbContext db, UserService userService)
    {
        _db = db;
        _userService = userService;
    }

    public async Task InitializeAsync()
    {
        Debug.Assert(ViscoinConfig.Perks.Count < 50 );
        Debug.Assert(ViscoinConfig.Items.Count < 50 );
        
        _db.ItemEntities.CompareAndUpdate(ViscoinConfig.Items);
        _db.PerkEntities.CompareAndUpdate(ViscoinConfig.Perks);

        await _db.SaveChangesAsync();
    }

    public async Task<int> GetPerkQty(IUser discordUser, string perkId)
    {
        var user = await _userService.GetOrCreateUser(discordUser,
            x => x.Include(userEntity => userEntity.Inventory)
                .ThenInclude(inventoryEntity => inventoryEntity.Perks).AsNoTracking());

        return user.Inventory.Perks.Count(x => x.PerkId == perkId);
    }
}

public static class InventoryDbExtension
{
    public static void CompareAndUpdate<TEntity>(this DbSet<TEntity> set, List<TEntity> compareTo) where TEntity : class, IInventoryDataEntity
    {
        var itemsInDb = set.Select(x => x.Id).ToList();

        foreach (var entity in compareTo)
        {
            var contains = itemsInDb.Any(o => o == entity.Id);
            if (!contains)
            {
                set.Add(entity);
            }
        }
    }
}