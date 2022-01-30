using Microsoft.EntityFrameworkCore;
using Viscoin.Bot.Features.Statistics.Types;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.Statistics;

public class StatService
{
    private readonly ApplicationDbContext _db;

    public StatService(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<UserEntity> GetRichestUsers()
    {
        return _db.Users.OrderByDescending(x => x.Balance).AsNoTracking().ToList();
    }

    public async Task<List<MostActiveUsers>> GetMostActiveUsersAsync(TimeSpan timespan)
    {
        var data = await _db.QueryAsync<MostActiveUsers>(@"
                SELECT COUNT(*) TimesUsed, ""UserId""
                FROM ""CommandsExecuted""
                WHERE ""TimeExecuted"" > NOW() - @Timespan
                GROUP BY ""UserId""
                ORDER BY TimesUsed desc
        ", new { Timespan = timespan});

        return data.ToList();
    }
}