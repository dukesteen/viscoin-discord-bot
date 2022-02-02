using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using SkiaSharp;
using Viscoin.Bot.Features.Games.Woordle.Types;
using Viscoin.Bot.Features.Preconditions.Cooldown;
using Viscoin.Bot.Features.Preconditions.GamblingChannel;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Games.Woordle;

public class WoordleModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractiveService _interactiveService;
    private readonly WoordleService _woordleService;
    private readonly UserService _userService;
    private readonly Random _random;

    public WoordleModule(InteractiveService interactiveService, WoordleService woordleService, UserService userService, Random random)
    {
        _interactiveService = interactiveService;
        _woordleService = woordleService;
        _userService = userService;
        _random = random;
    }

    [SlashCommand("woordle", "raad de woordle")]
    [Cooldown(seconds: 5)]
    [RequireGamblingChannel]
    public async Task StartWoordle()
    {
        await DeferAsync();
        var user = await _userService.GetOrCreateUser(Context.User);
        var pickedWord = WoordleStaticData.PuzzleWords.ElementAt(_random.Next(WoordleStaticData.PuzzleWords.Count));
        var game = new WoordleGame(user, pickedWord);
        
        var image = new WoordleImageBuilder(new SKImageInfo(400, 474));
        image.DrawRows(game.Choices);

        var imageUrl = await UploadImageGetUrlAsync(Context, image.BuildStream());
        
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Woordle")
            .WithDescription($"Typ een woord in de chat om te raden")
            .WithImageUrl(imageUrl);
        
        await FollowupAsync(embed: embedBuilder.Build());

        while (game.Choices.Count < 6)
        {
            var result = await _interactiveService.NextMessageAsync(x => x.Content.Length == 5 && x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromMinutes(3));

            if (result.IsTimeout)
            {
                await FollowupAsync(Context.User.Mention, embed: EmbedUtilities.CreateErrorEmbed("Je hebt niet optijd antwoord gegeven"));
            }
            
            if (result.Value == null)
            {
                return;
            }

            var wordChosen = false;
            
            foreach (var prevChoice in game.Choices)
            {
                if (result.Value.Content == prevChoice.Pick)
                {
                    await FollowupAsync(embed: EmbedUtilities.CreateErrorEmbed("Je hebt dit woord al een keer gekozen"));
                    wordChosen = true;
                }
            }

            if (wordChosen) continue;
            
            if (!WoordleStaticData.AllowedWords.Contains(result.Value.Content.ToLower()))
            {
                await FollowupAsync(embed: EmbedUtilities.CreateErrorEmbed("Dit woord staat niet in de woordenlijst"));
                continue;
            }
        
            var choice = _woordleService.GetChoiceFromString(result.Value.Content, game.PickedWord);
        
            game.Choices.Add(choice);
        
            image = new WoordleImageBuilder(new SKImageInfo(400, 474));
            image.DrawRows(game.Choices);

            imageUrl = await UploadImageGetUrlAsync(Context, image.BuildStream());

            if (choice.Correct)
            {
                var coinAmount = (6 - game.Choices.Count + 1) * 200;
                
                embedBuilder = new EmbedBuilder()
                    .WithTitle("Woordle")
                    .WithDescription($"Je hebt het woord goed geraden en {coinAmount} {AppConstants.ViscoinEmote} verdiend")
                    .WithImageUrl(imageUrl);
                
                await FollowupAsync(Context.User.Mention, embed: embedBuilder.Build());

                await _userService.AddCoinsAsync(user, coinAmount);
                
                return;
            }

            if (game.Choices.Count == 6)
            {
                embedBuilder = new EmbedBuilder()
                    .WithTitle("Woordle")
                    .WithDescription($"Helaas! Je hebt het woord niet geraden. Het woord was `{game.PickedWord}`")
                    .WithImageUrl(imageUrl);
            }
            else
            {
                embedBuilder = new EmbedBuilder()
                    .WithTitle("Woordle")
                    .WithDescription($"Typ een woord in de chat om te raden")
                    .WithImageUrl(imageUrl);
            }
            
            await FollowupAsync(Context.User.Mention, embed: embedBuilder.Build());
        }
    }
    
    private async Task<string> UploadImageGetUrlAsync(SocketInteractionContext ctx, Stream imageAsStream)
    {
        var channel = await ctx.Client.GetChannelAsync(AppConstants.BotDepositChannel) as ISocketMessageChannel;
        var message = await channel?.SendFileAsync(imageAsStream, $"{Guid.NewGuid():N}.png")!;
        return message.Attachments.First().Url;
    }
}