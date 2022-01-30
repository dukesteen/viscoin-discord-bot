using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Viscoin.Bot.Features.Games.Mines.Types;
using Viscoin.Bot.Features.Preconditions.GamblingChannel;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Games.Mines;

public class MinesModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly UserService _userService;
    private readonly MinesService _mines;

    public MinesModule(UserService userService, MinesService mines)
    {
        _userService = userService;
        _mines = mines;
    }

    [SlashCommand("mines", "start een mines game")]
    [RequireGamblingChannel]
    public async Task StartMines([MinValue(1)][MaxValue(24)] int bombs, [MinValue(1)] int amount)
    {
        await DeferAsync();
        
        var user = await _userService.GetOrCreateUser(Context.User);

        if (user.Balance < amount)
        {
            await FollowupAsync(embed: EmbedConstants.NotEnoughBalanceEmbed, ephemeral: true);
            return;
        }

        var game = _mines.CreateMinesGame(user.ServerSeed.ToString(), user.ClientSeed, user.Nonce, bombs, amount, user);
        
        await _userService.RemoveCoinsAsync(user, amount);
        await _userService.IncreaseNonceAsync(user);

        var componentBuilder = GetMineFieldComponentBuilder(game.Minefield, game.Id, false);
        var embedBuilder = GetMinesEmbedBuilder();

        var message = await FollowupAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());

        game.GameMessage = message;

        var cashoutComponentBuilder = GetCashoutComponentBuilder(game);

        message = await Context.Channel.SendMessageAsync("‎", components: cashoutComponentBuilder.Build());

        game.CashoutMessage = message;

        _mines.UpdateGame(game);
    }

    [ComponentInteraction("mines:*:*/*")]
    public async Task UncoverMine(string gameId, string arg1, string arg2)
    {
        var game = _mines.GetMinesGameById(gameId);
        var originalMessage = Context.Interaction as SocketMessageComponent;

        if (game == null)
        {
            await originalMessage?.Message.DeleteAsync()!;

            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Deze game kon niet gevonden worden in memory"));
            return;
        }

        if (Context.User.Id != game.Player.Id)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Dit is niet jouw game"), ephemeral: true);
            return;
        }

        if (game.Status == MinesGameStatus.Cashed)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Deze game is al klaar"));
            return;
        }
        
        int y = int.Parse(arg1);
        int x = int.Parse(arg2);

        if (game.Minefield[y][x].Status == MineStatus.Discovered)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Deze mine is al gevonden"), ephemeral: true);
            return;
        }
        
        game.Minefield[y][x].Status = MineStatus.Discovered;
        
        var embedBuilder = GetMinesEmbedBuilder();
        ComponentBuilder? componentBuilder;

        if (game.Minefield[y][x].Type == MineType.Bomb)
        {
            game.Status = MinesGameStatus.Dead;
            embedBuilder = new EmbedBuilder()
                .WithTitle("Kaboom")
                .WithDescription($"Je hebt een bom geraakt en {game.Amount} {AppConstants.ViscoinEmote} verloren");
            
            for (int i = 0; i < game.Minefield.Length; i++)
            {
                for (int j = 0; j < game.Minefield[i].Length; j++)
                {
                    game.Minefield[i][j].Status = MineStatus.Discovered;
                }
            }
            
            componentBuilder = GetMineFieldComponentBuilder(game.Minefield, game.Id, game.Status == MinesGameStatus.Dead);

            await DeferAsync();

            await Context.Interaction.ModifyOriginalResponseAsync(messageProperties =>
            {
                messageProperties.Embed = embedBuilder.Build();
                messageProperties.Components = componentBuilder.Build();
            });
            
            await game.CashoutMessage.DeleteAsync();

            return;
        }
        
        game.DiscoveredTiles += 1;
        var multiplier = _mines.GetMinesMultiplier(game);
        game.Minefield[y][x].Multiplier = multiplier;
        game.CurrentMultiplier = multiplier;
        
        _mines.UpdateGame(game);
        
        componentBuilder = GetMineFieldComponentBuilder(game.Minefield, game.Id, game.Status == MinesGameStatus.Dead);

        await DeferAsync();
        
        await Context.Interaction.ModifyOriginalResponseAsync(messageProperties =>
        {
            messageProperties.Embed = embedBuilder.Build();
            messageProperties.Components = componentBuilder.Build();
        });
    }

    [ComponentInteraction("mines:*:cashout")]
    public async Task CashoutMinesGame(string gameId)
    {
        var game = _mines.GetMinesGameById(gameId);

        if (game == null)
        {
            await game?.CashoutMessage.DeleteAsync()!;
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Deze game bestaat niet meer"));
            return;
        }
        
        if (Context.User.Id != game.Player.Id)
        {
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Dit is niet jouw game"), ephemeral: true);
            return;
        }

        if (game.Status == MinesGameStatus.Dead)
        {
            await game.CashoutMessage.DeleteAsync()!;
            await RespondAsync(embed: EmbedUtilities.CreateErrorEmbed("Er is een mine geraakt in deze game en je kan niet meer cashen"), ephemeral: true);
            return;
        }

        game.Status = MinesGameStatus.Cashed;
        
        for (int i = 0; i < game.Minefield.Length; i++)
        {
            for (int j = 0; j < game.Minefield[i].Length; j++)
            {
                game.Minefield[i][j].Status = MineStatus.Discovered;
            }
        }
        
        var componentBuilder = GetMineFieldComponentBuilder(game.Minefield, game.Id, game.Status == MinesGameStatus.Cashed);
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Je hebt gecashed!")
            .WithDescription(
                $"Er zijn {Math.Round(game.Amount * game.CurrentMultiplier - game.Amount, 0)} {AppConstants.ViscoinEmote} toegevoegd aan je account");

        var user = await _userService.GetOrCreateUser(Context.User);
        await _userService.AddCoinsAsync(user, (int)Math.Round(game.Amount * game.CurrentMultiplier));

        _mines.RemoveMinesGame(game);
        
        await DeferAsync();

        await game.GameMessage.ModifyAsync(messageProperties =>
        {
            messageProperties.Embed = embedBuilder.Build();
            messageProperties.Components = componentBuilder.Build();
        });

        await game.CashoutMessage.DeleteAsync();
    }
    
    private ComponentBuilder GetMineFieldComponentBuilder(Mine[][] mineField, string gameId, bool dead)
    {
        var componentBuilder = new ComponentBuilder();
        
        for (int i = 0; i < mineField.Length; i++)
        {
            for (int j = 0; j < mineField[i].Length; j++)
            {
                var mine = mineField[i][j];
                var button = new ButtonBuilder
                {
                    CustomId = $"mines:{gameId}:{i}/{j}",
                    Label = $" ",
                    Style = ButtonStyle.Primary,
                    IsDisabled = dead
                };

                if (mine.Type == MineType.Bomb && mine.Status == MineStatus.Discovered)
                {
                    button.Style = ButtonStyle.Danger;
                    button.Label = "💣";
                } else if(mine.Type == MineType.Default && mine.Status == MineStatus.Discovered)
                {
                    button.Style = ButtonStyle.Success;
                    if (mine.Multiplier > 0)
                    {
                        button.Label = $"{mineField[i][j].Multiplier}x";
                    }
                }
                else
                {
                    button.Style = ButtonStyle.Secondary;
                }

                componentBuilder.WithButton(button, i);
            }
        }

        return componentBuilder;
    }
    
    private EmbedBuilder GetMinesEmbedBuilder()
    {
        return new EmbedBuilder()
            .WithTitle("Mines")
            .WithDescription("Klik op de knoppen om mines te laten zien");
    }
    
    private ComponentBuilder GetCashoutComponentBuilder(MinesGame minesGame)
    {
        return new ComponentBuilder()
            .WithButton($"Cash out", $"mines:{minesGame.Id}:cashout");
    }
}