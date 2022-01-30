using Discord;
using Viscoin.Bot.Features.Economy.Types;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.Economy;

public class EconomyService
{
    private readonly ApplicationDbContext _db;

    public EconomyService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddWordleEntry(IUser user, int wordle)
    {
        _db.WordleEntries.Add(new WordleEntry(user.Id, wordle));
        await _db.SaveChangesAsync();
    }

    public bool HasCompletedWordle(IUser contextUser, int wordle)
    {
        var result = _db.WordleEntries.FirstOrDefault(x => x.DiscordId == contextUser.Id && x.WordleId == wordle);
        if (result == null)
        {
            return false;
        }

        return true;
    }
}