using Discord;
using Microsoft.EntityFrameworkCore;
using Viscoin.Bot.Features.Inventory.Types;
using Viscoin.Bot.Features.Statistics.Types;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.User;

public class UserService
{
    private readonly ApplicationDbContext _db;

    public UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<UserEntity> GetOrCreateUser(IUser user, Func<DbSet<UserEntity>, IQueryable<UserEntity>> query)
    {
        if (user is IGuildUser guildUser)
        {
            var userEntity = query(_db.Users).FirstOrDefault(x => x.Id == user.Id);

            if (userEntity == null)
            {
                userEntity = new UserEntity(guildUser.Id);
                _db.Users.Add(userEntity);
                await _db.SaveChangesAsync();
            }

            userEntity.Nickname = guildUser.Nickname;
            userEntity.Username = guildUser.Username;

            return userEntity;
        }
        
        throw new Exception("User is not a guild user");
    }
    
    public async Task<UserEntity> GetOrCreateUser(IUser user)
    {
        if (user is IGuildUser guildUser)
        {
            var userEntity = _db.Users.FirstOrDefault(x => x.Id == user.Id);

            if (userEntity == null)
            {
                userEntity = new UserEntity(guildUser.Id);
                _db.Users.Add(userEntity);
                await _db.SaveChangesAsync();
            }

            userEntity.Nickname = guildUser.Nickname;
            userEntity.Username = guildUser.Username;

            return userEntity;
        }
        
        throw new Exception("User is not a guild user");
    }
    
    private async Task<UserEntity> Patch(UserEntity user, Action<UserEntity> act)
    {
        act.Invoke(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<UserEntity> AddCoinsAsync(UserEntity user, int amount)
    {
        _db.BalanceUpdates.Add(new BalanceUpdate(user.Balance + amount, amount, DateTime.Now, user.Id));
        return await Patch(user, x => x.Balance += amount);
    }

    public async Task<UserEntity> RemoveCoinsAsync(UserEntity user, int amount)
    {
        _db.BalanceUpdates.Add(new BalanceUpdate(user.Balance - amount, -amount, DateTime.Now, user.Id));
        return await Patch(user, x => x.Balance -= amount);
    }

    public async Task<UserEntity> ResetCoinsAsync(UserEntity user)
        => await Patch(user, x => x.Balance = 0);

    public async Task<UserEntity> IncreaseNonceAsync(UserEntity user)
        => await Patch(user, x => x.Nonce++);

    public async Task<UserEntity> AddPerksAsync(UserEntity user, List<InventoryPerkEntity> perks)
        => await Patch(user, x => x.Inventory.Perks.AddRange(perks));

    public async Task<UserEntity> RotateServerSeedAsync(UserEntity user)
        => await Patch(user, x =>
        {
            x.ServerSeed = x.NextServerSeed;
            x.NextServerSeed = Guid.NewGuid();
        });

    public async Task<UserEntity> SetClientSeedAsync(UserEntity user, string seed)
    {
        user = await Patch(user, x => x.ClientSeed = seed);
        user = await RotateServerSeedAsync(user);

        return user;
    }
}