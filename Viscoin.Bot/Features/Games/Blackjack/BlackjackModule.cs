using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using SkiaSharp;
using Viscoin.Bot.Features.Preconditions.GamblingChannel;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Games.Blackjack;

public class BlackjackModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserService _userService;
    private readonly BlackjackService _blackjack;

    public BlackjackModule(UserService userService, BlackjackService blackjack)
    {
        _userService = userService;
        _blackjack = blackjack;
    }

    [SlashCommand("blackjack", "start een blackjack game")]
    [RequireGamblingChannel]
    public async Task StartBlackjack([MinValue(1)] int amount, [MinValue(1)] [MaxValue(10)] int shuffles = 1)
    {
        await DeferAsync();
        
        var user = await _userService.GetOrCreateUser(Context.User);

        if (user.Balance < amount)
        {
            await FollowupAsync(embed: EmbedConstants.NotEnoughBalanceEmbed);
            return;
        }

        await _userService.RemoveCoinsAsync(user, amount);

        var game = _blackjack.StartGame(user.ServerSeed.ToString(), user.ClientSeed, user.Nonce, amount,
            user, shuffles);
        
        var embedBuilder = new EmbedBuilder();
        ComponentBuilder componentBuilder;

        if (game.PlayerCards.CalculateBjTotal() == 21)
        {
            var builder = new BlackjackImageBuilder(BlackjackConstants.ImageInfo)
                .DrawHeaders()
                .FromCardList(game.PlayerCards, CardSide.Player)
                .FromCardList(game.DealerCards, CardSide.Dealer);

            var imageUrl = await UploadImageGetUrlAsync(Context, builder.BuildStream());
            
            embedBuilder
                .WithTitle("Winner winner, chicken dinner!")
                .WithDescription($"Je hebt **21** gehaald en {amount * 1.5} {AppConstants.ViscoinEmote} verdiend")
                .AddField("Player", game.PlayerCards.GetDisplayString(), true)
                .AddField("Dealer", game.DealerCards.GetDisplayString(), true)
                .WithImageUrl(imageUrl);

            componentBuilder = GetBlackjackButtons(game.Id, true);

            await _userService.AddCoinsAsync(user, (int)(game.Amount * 2.5));
            
            _blackjack.RemoveGame(game);
        }
        else
        {
            var builder = new BlackjackImageBuilder(BlackjackConstants.ImageInfo)
                .DrawHeaders()
                .FromCardList(game.PlayerCards, CardSide.Player)
                .FromCardList(game.DealerCards, CardSide.Dealer, true);

            var imageUrl = await UploadImageGetUrlAsync(Context, builder.BuildStream());

            embedBuilder
                .WithTitle("Blackjack!")
                .WithDescription($"Neem nog een kaart, of pas")
                .AddField("Player", game.PlayerCards.GetDisplayString(), true)
                .AddField("Dealer", game.DealerCards.GetMaskedDisplayString(), true)
                .WithFooter("J, V, H = 10, A = 1 of 11")
                .WithImageUrl(imageUrl);

            componentBuilder = GetBlackjackButtons(game.Id);
        }

        await _userService.IncreaseNonceAsync(user);
        
        await FollowupAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());
    }

    [ComponentInteraction("blackjack:*:hit")]
    public async Task HitCard(string gameId)
    {
        var originalMessage = Context.Interaction as SocketMessageComponent;

        if (originalMessage == null)
        {
            await RespondAsync(
                embed: EmbedUtilities.CreateErrorEmbed("Originele game message kon niet gevonden worden"));
        }

        var game = _blackjack.GetGame(gameId);

        if (game == null)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Game kon niet gevonden worden in memory"));
            await originalMessage?.Message.DeleteAsync()!;
            return;
        }
        
        if (game.Player.Id != Context.User.Id)
        {
            await RespondAsync("Dit is niet jouw game", ephemeral: true);
            return;
        }

        var user = await _userService.GetOrCreateUser(Context.User);
        
        game.PlayerCards.Add(game.Deck.Next());
        
        var embedBuilder = new EmbedBuilder();
        ComponentBuilder componentBuilder;
        
        if (game.PlayerCards.CalculateBjTotal() == 21)
        {
            while (game.DealerCards.CalculateBjTotal() < 17)
            {
                game.DealerCards.Add(game.Deck.Next());
            }
            
            var builder = new BlackjackImageBuilder(BlackjackConstants.ImageInfo)
                .DrawHeaders()
                .FromCardList(game.PlayerCards, CardSide.Player)
                .FromCardList(game.DealerCards, CardSide.Dealer);

            var imageUrl = await UploadImageGetUrlAsync(Context, builder.BuildStream());
            
            embedBuilder
                .WithTitle("Winner winner, chicken dinner!")
                .WithDescription($"Je hebt **21** gehaald en {game.Amount} {AppConstants.ViscoinEmote} verdiend")
                .AddField("Player", game.PlayerCards.GetDisplayString(), true)
                .AddField("Dealer", game.DealerCards.GetDisplayString(), true)
                .WithImageUrl(imageUrl);

            componentBuilder = GetBlackjackButtons(game.Id, true);

            await _userService.AddCoinsAsync(user, game.Amount * 2);
        }
        else if (game.PlayerCards.CalculateBjTotal() > 21)
        {
            while (game.DealerCards.CalculateBjTotal() < 17)
            {
                game.DealerCards.Add(game.Deck.Next());
            }
            
            var builder = new BlackjackImageBuilder(BlackjackConstants.ImageInfo)
                .DrawHeaders()
                .FromCardList(game.PlayerCards, CardSide.Player)
                .FromCardList(game.DealerCards, CardSide.Dealer);

            var imageUrl = await UploadImageGetUrlAsync(Context, builder.BuildStream());
            
            embedBuilder
                .WithTitle("Verloren!")
                .WithDescription($"Je bent boven de **21** gegaan en hebt {game.Amount} {AppConstants.ViscoinEmote} verloren")
                .AddField("Player", game.PlayerCards.GetDisplayString(), true)
                .AddField("Dealer", game.DealerCards.GetDisplayString(), true)
                .WithImageUrl(imageUrl);

            componentBuilder = GetBlackjackButtons(game.Id, true);
        }
        else if (game.PlayerCards.Count >= 5)
        {
            var builder = new BlackjackImageBuilder(BlackjackConstants.ImageInfo)
                .DrawHeaders()
                .FromCardList(game.PlayerCards, CardSide.Player)
                .FromCardList(game.DealerCards, CardSide.Dealer);

            var imageUrl = await UploadImageGetUrlAsync(Context, builder.BuildStream());
            
            embedBuilder
                .WithTitle("Gewonnen!")
                .WithDescription($"Je hebt 5 kaarten gehaald en {game.Amount} {AppConstants.ViscoinEmote} gewonnen")
                .AddField("Player", game.PlayerCards.GetDisplayString(), true)
                .AddField("Dealer", game.DealerCards.GetDisplayString(), true)
                .WithImageUrl(imageUrl);

            componentBuilder = GetBlackjackButtons(game.Id, true);
            
            await _userService.AddCoinsAsync(user, game.Amount * 2);
        }
        else
        {
            var builder = new BlackjackImageBuilder(BlackjackConstants.ImageInfo)
                .DrawHeaders()
                .FromCardList(game.PlayerCards, CardSide.Player)
                .FromCardList(game.DealerCards, CardSide.Dealer, true);

            var imageUrl = await UploadImageGetUrlAsync(Context, builder.BuildStream());
            
            embedBuilder
                .WithTitle("Blackjack!")
                .WithDescription($"Neem nog een kaart, of pas")
                .AddField("Player", game.PlayerCards.GetDisplayString(), true)
                .AddField("Dealer", game.DealerCards.GetMaskedDisplayString(), true)
                .WithFooter("J, V, H = 10, A = 1 of 11")
                .WithImageUrl(imageUrl);

            componentBuilder = GetBlackjackButtons(game.Id);
        }
        
        _blackjack.UpdateGame(game);

        await DeferAsync();
        
        await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Components = componentBuilder.Build();
                x.Embed = embedBuilder.Build();
            }
        );
    }

    [ComponentInteraction("blackjack:*:stand")]
    public async Task BlackjackStand(string gameId)
    {
        var originalMessage = Context.Interaction as SocketMessageComponent;

        if (originalMessage == null)
        {
            await RespondAsync(
                embed: EmbedUtilities.CreateErrorEmbed("Originele game message kon niet gevonden worden"));
        }

        var game = _blackjack.GetGame(gameId);

        if (game == null)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Game kon niet gevonden worden in memory"));
            await originalMessage?.Message.DeleteAsync()!;
            return;
        }
        
        if (game.Player.Id != Context.User.Id)
        {
            await RespondAsync("Dit is niet jouw game", ephemeral: true);
            return;
        }

        var user = await _userService.GetOrCreateUser(Context.User);
        
        var embedBuilder = new EmbedBuilder();
        var componentBuilder = GetBlackjackButtons(game.Id, true);
        
        while (game.DealerCards.CalculateBjTotal() < 17)
        {
            game.DealerCards.Add(game.Deck.Next());
        }
        
        var playerTotal = game.PlayerCards.CalculateBjTotal();
        var dealerTotal = game.DealerCards.CalculateBjTotal();
        
        var builder = new BlackjackImageBuilder(BlackjackConstants.ImageInfo)
            .DrawHeaders()
            .FromCardList(game.PlayerCards, CardSide.Player)
            .FromCardList(game.DealerCards, CardSide.Dealer);

        var imageUrl = await UploadImageGetUrlAsync(Context, builder.BuildStream());
        
        if (playerTotal > dealerTotal || dealerTotal > 21)
        {
            string descriptionMessage;
            if (dealerTotal > 21)
            {
                descriptionMessage =
                    $"De dealer is boven de **21** gegaan. Je hebt {game.Amount} {AppConstants.ViscoinEmote} gewonnen";
            }
            else
            {
                descriptionMessage =
                    $"Je had een totaal dat hoger was dan de dealer. Je hebt {game.Amount} {AppConstants.ViscoinEmote} gewonnen";
            }
            
            embedBuilder
                .WithTitle("Gewonnen!")
                .WithDescription(descriptionMessage)
                .AddField("Player", game.PlayerCards.GetDisplayString(), true)
                .AddField("Dealer", game.DealerCards.GetDisplayString(), true)
                .WithImageUrl(imageUrl);

            await _userService.AddCoinsAsync(user, game.Amount * 2);
        } else if (playerTotal == dealerTotal)
        {
            embedBuilder
                .WithTitle("Gelijkspel!")
                .WithDescription($"Jij en de dealer hebben allebei hetzelfde totaal, je hebt geen coins verloren")
                .AddField("Player", game.PlayerCards.GetDisplayString(), true)
                .AddField("Dealer", game.DealerCards.GetDisplayString(), true)
                .WithImageUrl(imageUrl);

            await _userService.AddCoinsAsync(user, game.Amount);
        } else
        {
            embedBuilder
                .WithTitle("Verloren!")
                .WithDescription($"De dealer had een hoger totaal dan jij. Je hebt {game.Amount} {AppConstants.ViscoinEmote} verloren")
                .AddField("Player", game.PlayerCards.GetDisplayString(), true)
                .AddField("Dealer", game.DealerCards.GetDisplayString(), true)
                .WithImageUrl(imageUrl);
        }
        
        await DeferAsync();
        
        await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Components = componentBuilder.Build();
                x.Embed = embedBuilder.Build();
            }
        );
        
        _blackjack.RemoveGame(game);
    }

    private async Task<string> UploadImageGetUrlAsync(SocketInteractionContext ctx, Stream imageAsStream)
    {
        var channel = await ctx.Client.GetChannelAsync(AppConstants.BotDepositChannel) as ISocketMessageChannel;
        var message = await channel?.SendFileAsync(imageAsStream, $"{Guid.NewGuid():N}.png")!;
        return message.Attachments.First().Url;
    }
    
    private ComponentBuilder GetBlackjackButtons(string gameId, bool disabled = false)
    {
        return new ComponentBuilder()
            .WithButton("Hit", $"blackjack:{gameId}:hit", disabled: disabled)
            .WithButton("Stand", $"blackjack:{gameId}:stand", disabled: disabled);
    }
}

public static class BlackjackConstants
{
    public static readonly SKImageInfo ImageInfo = new SKImageInfo(1200, 500);
}