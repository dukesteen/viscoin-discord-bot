using Discord;
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
    
    public async Task<List<BalanceHistoryQuery>> GetBalanceHistory(TimeSpan timespan, IUser user)
    {
        var userId = long.Parse(user.Id.ToString());
        
        var data = await _db.QueryAsync<BalanceHistoryQuery>(@"
            SELECT time_bucket_gapfill(@Timespan, ""Timestamp"", now() - INTERVAL '5 years', now()) as Time, locf(ROUND(last(""ResultingBalance"", ""Timestamp""))) as Balance
            FROM ""BalanceUpdates""
            WHERE ""UserId"" = @UserId
            GROUP BY Time
            ORDER BY Time DESC
            LIMIT 15
        ", new { Timespan = timespan, UserId = userId});

        return data.ToList();
    }
}