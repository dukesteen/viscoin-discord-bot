namespace Viscoin.Bot.Features.Games.Mines.Types;

public class Mine
{
    public MineStatus Status { get; set; }
    public MineType Type { get; set; }
    public bool DeathMine { get; set; }

    private double _multiplier;
    public double? Multiplier
    {
        get => Math.Round(_multiplier, 2);
        set => _multiplier = Math.Round(value ?? 1, 2);
    }
}