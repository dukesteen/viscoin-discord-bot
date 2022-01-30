using Discord;
using Viscoin.Bot.Features.Preconditions.GamblingChannel;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.Admin;

public class AdminService
{
    private readonly ApplicationDbContext _db;

    public AdminService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> SetGambling(IGuildChannel channel)
    {

        var entry = await _db.GamblingChannels.FindAsync(channel.Id);

        if (entry == null)
        {
            _db.GamblingChannels.Add(new GamblingChannel(channel.Id));
            await _db.SaveChangesAsync();
            return true;
        }

        return false;
    }
}