using SkiaSharp;
using Viscoin.Bot.Features.User;

namespace Viscoin.Bot.Features.Games.Woordle.Types;

public class WoordleGame
{
    public WoordleGame(UserEntity player, string pickedWord)
    {
        Player = player;
        PickedWord = pickedWord;
    }

    public UserEntity Player { get; set; }
    public string PickedWord { get; set; }
    public List<WoordleChoice> Choices { get; set; } = new();
}

public class WoordleChoice
{
    public WoordleChoice(string pick, List<WoordleCharacter> characters)
    {
        Pick = pick;
        Characters = characters;
    }

    public string Pick { get; set; }
    public List<WoordleCharacter> Characters { get; set; }
    public bool Correct { get; set; } = false;
}

public class WoordleCharacter
{
    public WoordleCharacter(char character, WoordleStatus status)
    {
        Character = character;
        Status = status;
    }
    
    public char Character { get; set; }
    public WoordleStatus Status { get; set; }
}

public enum WoordleStatus
{
    WrongCharacter,
    WrongPosition,
    RightPosition
}

public static class WoordleExtensions 
{
    public static SKColor GetColor(this WoordleStatus status)
    {
        switch (status)
        {
            case WoordleStatus.RightPosition:
                return SKColor.Parse("538D4E");
            case WoordleStatus.WrongPosition:
                return SKColor.Parse("B59F3B");
            default:
                return SKColor.Parse("3A3A3C");
        }
    }
}