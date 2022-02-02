using Viscoin.Bot.Features.Games.Woordle.Types;

namespace Viscoin.Bot.Features.Games.Woordle;

public class WoordleService
{
    public WoordleChoice GetChoiceFromString(string guess, string correctWord)
    {
        var charList = new List<WoordleCharacter>();

        var guessChars = guess.ToCharArray();
        var correctWordChars = correctWord.ToCharArray();

        for (int i = 0; i < guessChars.Length; i++)
        {
            if (correctWordChars[i] == guessChars[i])
            {
                charList.Add(new WoordleCharacter(guessChars[i], WoordleStatus.RightPosition));
            } 
            else if (correctWordChars.Contains(guessChars[i]))
            {
                charList.Add(new WoordleCharacter(guessChars[i], WoordleStatus.WrongPosition));
            } 
            else
            {
                charList.Add(new WoordleCharacter(guessChars[i], WoordleStatus.WrongCharacter));
            }
        }
        
        for (int i = 0; i < guessChars.Length; i++)
        {
            if (charList[i].Status == WoordleStatus.WrongPosition)
            {
                if (charList.Count(x => x.Character == guessChars[i]) > 1)
                {
                    if (correctWord.Count(x => x == guessChars[i]) == 1)
                    {
                        charList[i].Status = WoordleStatus.WrongCharacter;
                    }
                }
            }
        }

        var choice = new WoordleChoice(guess, charList);
        
        if (guess.ToLower() == correctWord)
        {
            choice.Correct = true;
        }
        
        return choice;
    }
}