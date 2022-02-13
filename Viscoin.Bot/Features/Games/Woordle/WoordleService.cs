using Viscoin.Bot.Features.Games.Woordle.Types;

namespace Viscoin.Bot.Features.Games.Woordle;

public class WoordleService
{
    public WoordleChoice GetChoiceFromString(string guess, string correctWord)
    {
        var charArray = new WoordleCharacter[5];

        var guessChars = guess.ToCharArray();
        var correctWordChars = correctWord.ToCharArray();

        var temp = correctWordChars;

        for (int i = 0; i < 5; i++)
        {
            if (correctWordChars[i] == guessChars[i])
            {
                charArray[i] = new WoordleCharacter(guessChars[i], WoordleStatus.RightPosition);
                temp[i] = '_';
            }
        }

        for (int i = 0; i < 5; i++)
        {
            var letter = temp[i];

            if (letter == '_')
            {
            } else if (temp.Contains(guessChars[i]))
            {
                charArray[i] = new WoordleCharacter(guessChars[i], WoordleStatus.WrongPosition);
            }
            else
            {
                charArray[i] = new WoordleCharacter(guessChars[i], WoordleStatus.WrongCharacter);
            }
            
        }

        var choice = new WoordleChoice(guess, charArray.ToList());
        
        if (guess.ToLower() == correctWord)
        {
            choice.Correct = true;
        }
        
        return choice;
    }
}