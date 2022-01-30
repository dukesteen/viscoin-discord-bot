using Discord;
using Viscoin.Bot.Features.User;

namespace Viscoin.Bot.Features.Games.Mines.Types;

public class MinesGame
{
    public string Id { get; set; } = null!;
    public int Amount { get; set; }
    public UserEntity Player { get; set; } = null!;
    public Mine[][] Minefield { get; set; } = null!;
    public MinesGameStatus Status { get; set; }
    public int DiscoveredTiles { get; set; }
    public int Bombs { get; set; }
    private double _currentMultiplier;
    public double CurrentMultiplier
    {
        get => Math.Round(_currentMultiplier, 2);
        set => _currentMultiplier = Math.Round(value, 2);
    }
    
    public IUserMessage GameMessage { get; set; } = null!;
    public IUserMessage CashoutMessage { get; set; } = null!;
}

public enum MinesGameStatus
{
    Playing,
    Dead,
    Cashed
}