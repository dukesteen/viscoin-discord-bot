using Discord;
using Microsoft.EntityFrameworkCore;
using Viscoin.Bot.Features.Lottery.Types;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.Lottery;

public class LotteryService
{
    private readonly ApplicationDbContext _db;
    private readonly Random _random;

    public LotteryService(ApplicationDbContext db, Random random)
    {
        _db = db;
        _random = random;
    }

    public async Task<Types.Lottery> StartLotteryAsync(int initialAmount, int ticketPrice, int maxTickets)
    {
        var lottery = _db.Lotteries.Add(new Types.Lottery
        {
            MaxTickets = maxTickets,
            TicketPrice = ticketPrice,
            PrizePool = initialAmount,
            IsActive = true
        });

        await _db.SaveChangesAsync();

        return lottery.Entity;
    }

    public async Task SetMessageAsync(int lotteryId, ulong channelId, ulong messageId)
    {
        var lottery = _db.Lotteries.FirstOrDefault(x => x.Id == lotteryId);
        if (lottery == null)
            return;

        lottery.LotteryMessageId = messageId;
        lottery.LotteryMessageChannelId = channelId;

        await _db.SaveChangesAsync();
    }

    public Types.Lottery? GetLottery(int lotteryId)
    {
        return _db.Lotteries.FirstOrDefault(x => x.Id == lotteryId);
    }

    public int GetEntriesForUser(IUser contextUser, int lotteryId)
    {
        var lottery = _db.Lotteries.Include(x => x.Entries).FirstOrDefault(x => x.Id == lotteryId);
        var entry = lottery?.Entries.FirstOrDefault(x => x.UserId == contextUser.Id);
        if (entry == null)
        {
            return 0;
        }

        return entry.TicketAmount;
    }

    public async Task<Types.Lottery> PurchaseTicketsAsync(UserEntity user, int tickets, int lotteryId)
    {
        var lottery = _db.Lotteries.Include(x => x.Entries).First(x => x.Id == lotteryId);

        var entry = lottery.Entries.Find(x => x.UserId == user.Id);
        if (entry == null)
        {
            lottery.Entries.Add(new LotteryEntry
            {
                LotteryId = lotteryId,
                TicketAmount = tickets,
                UserId = user.Id
            });
            await _db.SaveChangesAsync();
        }
        else
        {
            entry.TicketAmount += tickets;
            await _db.SaveChangesAsync();
        }

        lottery.PrizePool += tickets * lottery.TicketPrice;
        await _db.SaveChangesAsync();
        return lottery;
    }

    public int GetTotalTicketsSold(int lotteryId)
    {
        var lottery = _db.Lotteries.Include(x => x.Entries).First(x => x.Id == lotteryId);

        return lottery.Entries.Sum(x => x.TicketAmount);
    }

    public async Task<List<LotteryEntry>> GetLotteryEntries(int lotteryId)
    {
        return (await _db.Lotteries.Include(c => c.Entries).FirstOrDefaultAsync(x => x.Id == lotteryId))!.Entries.ToList();
    }

    public async Task<ulong> RollWinnerAsync(int lotteryId)
    {
        var lottery = _db.Lotteries.Include(x => x.Entries).First(x => x.Id == lotteryId);
        
        var entryList = new List<ulong>();

        foreach (var entry in lottery.Entries)
        {
            for (int i = 0; i < entry.TicketAmount; i++)
            {
                entryList.Add(entry.UserId);
            }
        }

        var randomPick = _random.Next(entryList.Count);

        lottery.IsActive = false;
        await _db.SaveChangesAsync();
        
        return entryList[randomPick];
    }
}