using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Shared.Attributes;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Lottery;

[Group("lottery", "loterij commands")]
public class LotteryModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly LotteryService _lotteryService;
    private readonly UserService _userService;

    public LotteryModule(LotteryService lotteryService, UserService userService)
    {
        _lotteryService = lotteryService;
        _userService = userService;
    }

    [SlashCommand("start", "start een loterij")]
    [RequireAdmin]
    public async Task StartLottery(int initialAmount, int ticketPrice, int maxTickets)
    {
        var lottery = await _lotteryService.StartLotteryAsync(initialAmount, ticketPrice, maxTickets);

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Visloterij")
            .AddField("Prize pool", $"{lottery.PrizePool} {AppConstants.ViscoinEmote}", true)
            .AddField("Ticket price: ", $"{lottery.TicketPrice} {AppConstants.ViscoinEmote}", true)
            .AddField("Max tickets", $"{lottery.MaxTickets} :tickets:", true )
            .AddField("Tickets verkocht", $"0 :tickets:", true)
            .WithThumbnailUrl("https://media.discordapp.net/attachments/611178267172405250/930227760780173363/visloterij.png?width=676&height=676")
            .WithFooter($"Id: {lottery.Id}")
            .WithColor(Color.Orange);
        
        var componentBuilder = new ComponentBuilder()
            .WithButton("Koop 1 lot", $"lottery:{lottery.Id}:1")
            .WithButton("Koop 10 loten", $"lottery:{lottery.Id}:10")
            .WithButton("Koop max loten", $"lottery:{lottery.Id}:max")
            .WithButton("Zie loten", $"lottery-viewtickets:{lottery.Id}", ButtonStyle.Secondary, row: 1)
            .WithButton("Info", $"lottery-info:{lottery.Id}", ButtonStyle.Secondary, row: 1);

        await DeferAsync();
        var message = await FollowupAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());

        await _lotteryService.SetMessageAsync(lottery.Id, message.Channel.Id, message.Id);
    }

    [ComponentInteraction("lottery:*:*", true)]
    public async Task BuyTickets(string arg1, string arg2)
    {
        var lotteryId = int.Parse(arg1);
        var lottery = _lotteryService.GetLottery(lotteryId);

        var user = await _userService.GetOrCreateUser(Context.User);

        if (Context.Interaction is SocketMessageComponent originalMessage)
        {
            if (lottery == null)
            {
                await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Deze loterij kon niet gevonden worden in de database"), ephemeral: true);
                return;
            }
            
            if (!lottery.IsActive)
            {
                await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Deze loterij is al afgelopen"), ephemeral: true);
                await originalMessage.Message.ModifyAsync(x => x.Components = new ComponentBuilder().Build());
                return;
            }
        
            if (arg2 == "max")
            {
                var userEntries = _lotteryService.GetEntriesForUser(Context.User, lotteryId);
                var totalPurchaseable = lottery.MaxTickets - userEntries;

                if (totalPurchaseable == 0)
                {
                    await RespondAsync(
                        embed: EmbedUtilities.CreateErrorEmbed(
                            $"Je hebt al {lottery.MaxTickets} :tickets: gekocht voor deze loterij"), ephemeral: true);
                    return;
                }

                if (user.Balance < totalPurchaseable * lottery.TicketPrice)
                {
                    await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Je hebt niet genoeg geld om dit te doen"), ephemeral: true);
                    return;
                }

                lottery = await _lotteryService.PurchaseTicketsAsync(user, totalPurchaseable, lotteryId);
                await _userService.RemoveCoinsAsync(user, totalPurchaseable * lottery.TicketPrice);

                await RespondAsync(embed: EmbedUtilities.CreateEmbedWithTitle("Loterij",
                    $"Je hebt {totalPurchaseable} :tickets: gekocht"), ephemeral: true);
            }
            else
            {
                var ticketAmount = int.Parse(arg2);

                var userEntries = _lotteryService.GetEntriesForUser(Context.User, lotteryId);
                
                if (userEntries + ticketAmount > lottery.MaxTickets)
                {
                    await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Je kan niet zoveel loten kopen voor deze loterij"), ephemeral: true);
                    return;
                }
            
                var totalPurchaseable = lottery.MaxTickets - userEntries;

                if (totalPurchaseable <= 0)
                {
                    await RespondAsync(
                        embed: EmbedUtilities.CreateErrorEmbed(
                            $"Je hebt al {lottery.MaxTickets} :tickets: gekocht voor deze loterij"), ephemeral: true);
                    return;
                }
            
                if (user.Balance < ticketAmount * lottery.TicketPrice)
                {
                    await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Je hebt niet genoeg geld om dit te doen"), ephemeral: true);
                    return;
                }
            
                lottery = await _lotteryService.PurchaseTicketsAsync(user, ticketAmount, lotteryId);
                await _userService.RemoveCoinsAsync(user, ticketAmount * lottery.TicketPrice);
            
                await RespondAsync(embed: EmbedUtilities.CreateEmbedWithTitle("Loterij",
                    $"Je hebt {ticketAmount} :tickets: gekocht"), ephemeral: true);
            }
        
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Visloterij")
                .AddField("Prize pool", $"{lottery.PrizePool} {AppConstants.ViscoinEmote}", true)
                .AddField("Ticket price: ", $"{lottery.TicketPrice} {AppConstants.ViscoinEmote}", true)
                .AddField("Max tickets", $"{lottery.MaxTickets} :tickets:", true )
                .AddField("Tickets verkocht", $"{_lotteryService.GetTotalTicketsSold(lotteryId)} :tickets:", true)
                .WithThumbnailUrl("https://media.discordapp.net/attachments/611178267172405250/930227760780173363/visloterij.png?width=676&height=676")
                .WithFooter($"Id: {lottery.Id}")
                .WithColor(Color.Orange);

            await originalMessage.Message.ModifyAsync(x => x.Embed = embedBuilder.Build());
        }
    }

    [ComponentInteraction("lottery-viewtickets:*", true)]
    public async Task ViewLotteryTickets(string arg1)
    {
        var lotteryId = int.Parse(arg1);
        var tickets = _lotteryService.GetEntriesForUser(Context.User, lotteryId);

        await RespondAsync(embed: EmbedUtilities.CreateEmbedWithTitle("Loterij", $"Je hebt {tickets} :tickets:"), ephemeral: true);
    }

    [ComponentInteraction("lottery-info:*", true)]
    public async Task LotteryInfo(string arg1)
    {
        var lotteryId = int.Parse(arg1);
        
        var lotteryEntries = await _lotteryService.GetLotteryEntries(lotteryId);
        var totalTickets = lotteryEntries.Sum(x => x.TicketAmount);

        var stringBuilder = new StringBuilder();
        
        foreach (var entry in lotteryEntries.OrderByDescending(x => x.TicketAmount))
        {
            stringBuilder.AppendLine($"<@{entry.UserId}>: {entry.TicketAmount} 🎟️ ({Math.Round(((double)entry.TicketAmount / totalTickets * 100d), 2)}%)");
        }
        
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Lottery info")
            .AddField("Total tickets", totalTickets)
            .WithDescription(stringBuilder.ToString())
            .WithFooter($"Id: {lotteryId}");

        await RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
    }

    [SlashCommand("roll", "roll een loterij")]
    [RequireAdmin]
    public async Task RollLottery(int lotteryId)
    {
        var lottery = _lotteryService.GetLottery(lotteryId);
        if (lottery == null)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Deze loterij bestaat niet"), ephemeral: true);
            return;
        }

        if (!lottery.IsActive)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Loterij is niet meer actief"), ephemeral: true);
            return;
        }

        var winnerId = await _lotteryService.RollWinnerAsync(lotteryId);
        var winnerUser = await Context.Channel.GetUserAsync(winnerId);
        var botUser = await _userService.GetOrCreateUser(winnerUser);
        await _userService.AddCoinsAsync(botUser, lottery.PrizePool);
        
        var embedBuilder = new EmbedBuilder()
            .WithTitle($"Winnaar van Visloterij {lottery.Id}")
            .AddField("Winner", $"{(winnerUser as SocketGuildUser)?.Nickname ?? winnerUser.Username}", true)
            .AddField("Prize", $"{lottery.PrizePool} {AppConstants.ViscoinEmote}", true)
            .WithThumbnailUrl("https://media.discordapp.net/attachments/611178267172405250/930227760780173363/visloterij.png?width=676&height=676")
            .WithColor(Color.Orange);

        await RespondAsync(embed: embedBuilder.Build());
    }
}