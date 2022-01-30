using System.Globalization;
using System.Net;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Newtonsoft.Json;
using Viscoin.Bot.Features.Inventory;
using Viscoin.Bot.Features.Preconditions.Cooldown;
using Viscoin.Bot.Features.Preconditions.GamblingChannel;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Economy;

public class EconomyModule : InteractionModuleBase<IInteractionContext>
{
    private readonly UserService _userService;
    private readonly InventoryService _inventoryService;
    private readonly InteractiveService _interactiveService;
    private readonly EconomyService _economyService;

    public EconomyModule(UserService userService, InteractiveService interactiveService, InventoryService inventoryService, EconomyService economyService)
    {
        _userService = userService;
        _interactiveService = interactiveService;
        _inventoryService = inventoryService;
        _economyService = economyService;
    }
    
    [SlashCommand("daily", "Dagelijkse coins")]
    [RequireGamblingChannel]
    [Cooldown(1)]
    public async Task Daily()
    {
        var user = await _userService.GetOrCreateUser((Context.User as IGuildUser)!);
        
        var dailyBoostPerks = await _inventoryService.GetPerkQty(Context.User, "DAILY_COIN_BOOST");
        
        var amount = 300 + 100 * dailyBoostPerks;

        await _userService.AddCoinsAsync(user, amount);
        
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Daily coins")
            .WithDescription($"Je hebt {amount} {AppConstants.ViscoinEmote} gekregen");

        await RespondAsync(embed: embedBuilder.Build());
    }

    [SlashCommand("beg", "smeek om coins bij de bot")]
    [RequireGamblingChannel]
    [Cooldown(0, 0, 5)]
    public async Task Beg()
    {
        var random = new Random();
        var user = await _userService.GetOrCreateUser(Context.User);

        var begChancePerks = await _inventoryService.GetPerkQty(Context.User, "BEG_CHANCE_BOOST");
        
        if(random.Next(100) < 20 + 20 * begChancePerks)
        {
            try
            {
                await _userService.AddCoinsAsync(user, 100);
                await RespondAsync($"Je hebt 100 {AppConstants.ViscoinEmote} gekregen");
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message);
            }
        } else
        {
            await RespondAsync("Volgende keer beter");
        }
    }
    
    [SlashCommand("trivia", "trivia vraag")]
    [RequireGamblingChannel]
    [Cooldown(0, 0, 5)]
    public async Task Trivia()
    {
        await DeferAsync();

        HttpClient client = new HttpClient();
        var res = await client.GetAsync("https://opentdb.com/api.php?amount=1&type=boolean");
        var data = JsonConvert.DeserializeObject<TriviaResponse>(await res.Content.ReadAsStringAsync());
        var question = data!.Results.FirstOrDefault();

        if (question == null)
        {
            await FollowupAsync(embed: EmbedUtilities.CreateErrorEmbed("Kon geen vraag vinden"));
            return;
        }
        
        bool correctAnswer = bool.Parse(question.CorrectAnswer);

        var perkAmt = await _inventoryService.GetPerkQty(Context.User, "TRIVIA_TIME_BOOST");
        
        var embedBuilder = new EmbedBuilder()
            .WithTitle($"Trivia ({10 + 5 * perkAmt} seconden)")
            .WithDescription(WebUtility.HtmlDecode(question.Question));
        var componentBuilder = new ComponentBuilder()
            .WithButton("True", "true")
            .WithButton("False", "false");
        var msg = await FollowupAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());

        var result = await _interactiveService.NextMessageComponentAsync(
            x => x.Message.Id == msg.Id && x.User.Id == Context.User.Id,
            timeout: TimeSpan.FromSeconds(10 + 5 * perkAmt));
        if (result.IsSuccess)
        {
            await result.Value.DeferAsync();
        }

        if (result.IsTimeout)
        {
            embedBuilder = new EmbedBuilder()
                .WithTitle("Timeout!")
                .WithDescription($"Je hebt de vraag niet snel genoeg beantwoord");

            componentBuilder = new ComponentBuilder();

            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = embedBuilder.Build();
                x.Components = componentBuilder.Build();
            });
            return;
        }

        var choice = bool.Parse(result.Value?.Data.CustomId!);
        if (choice == correctAnswer)
        {
            var user = await _userService.GetOrCreateUser(Context.User);

            perkAmt = await _inventoryService.GetPerkQty(Context.User, "TRIVIA_COIN_BOOST");
            
            var amount = 100 + 25 * perkAmt;

            await _userService.AddCoinsAsync(user, amount);

            embedBuilder = new EmbedBuilder()
                .WithTitle("Correct!")
                .WithDescription(
                    $"Je hebt het goede antwoord gegeven en {amount} {AppConstants.ViscoinEmote} verdiend");

            componentBuilder = new ComponentBuilder();

            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = embedBuilder.Build();
                x.Components = componentBuilder.Build();
            });
        }
        else
        {
            embedBuilder = new EmbedBuilder()
                .WithTitle("Fout!")
                .WithDescription($"Dit was niet het goede antwoord");
            componentBuilder = new ComponentBuilder();

            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = embedBuilder.Build();
                x.Components = componentBuilder.Build();
            });
        }
    }

    [SlashCommand("wordle", "submit je wordle")]
    public async Task SubmitWordle(string pick)
    {
        var user = await _userService.GetOrCreateUser(Context.User);
        var timespan = DateTime.Now - DateTime.ParseExact("19/06/2021", "dd/MM/yyyy", new CultureInfo("nl-NL"));
        var wordle = timespan.Days;

        if (_economyService.HasCompletedWordle(Context.User, wordle))
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Je hebt deze wordle al geraden"),
                ephemeral: true);
            return;
        }

        var words = JsonConvert.DeserializeObject<List<string>>(Resources.WordleItems);
        if (words == null)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Wordle woordenlijst kon niet gevonden worden"),
                ephemeral: true);
            return;
        }
        
        if (words[wordle] == pick.ToLower())
        {
            var perkQty = await _inventoryService.GetPerkQty(Context.User, "WORDLE_COIN_BOOST");
            var amount = 600 + 100 * perkQty;

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Goed!")
                .WithDescription(
                    $"{user.Nickname ?? user.Username} heeft de wordle geraden en {amount} {AppConstants.ViscoinEmote} verdiend");
            
            await _userService.AddCoinsAsync(user, 600 + 100 * perkQty);
            await _economyService.AddWordleEntry(Context.User, wordle);

            await RespondAsync("Goed!", ephemeral: true);
            
            await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
            return;
        }

        await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Wordle is niet goed"), ephemeral: true);

    }
    
}

public class Result
{
    [JsonProperty("category")]
    public string Category { get; set; } = null!;

    [JsonProperty("type")]
    public string Type { get; set; } = null!;

    [JsonProperty("difficulty")]
    public string Difficulty { get; set; } = null!;

    [JsonProperty("question")]
    public string Question { get; set; } = null!;

    [JsonProperty("correct_answer")]
    public string CorrectAnswer { get; set; } = null!;

    [JsonProperty("incorrect_answers")]
    public List<string> IncorrectAnswers { get; set; } = null!;
}

public class TriviaResponse
{
    [JsonProperty("response_code")]
    public int ResponseCode { get; set; }

    [JsonProperty("results")]
    public List<Result> Results { get; set; } = null!;
}