using System.Text;
using Viscoin.Bot.Shared.Types;

namespace Viscoin.Bot.Utilities;

public static class GamblingExtensionMethods
{
    public static int CalculateBjTotal(this List<Card> cards)
    {
        var total = 0;

        int aces = cards.Count(x => x.Rank == "A");
        
        foreach (var card in cards)
        {
            if (new []{ "J", "V", "H" }.Any(card.Rank.Contains))
            {
                total += 10;
            } else if (card.Rank != "A")
            {
                total += int.Parse(card.Rank);
            }
        }

        for (int i = 0; i < aces; i++)
        {
            if (total + 11 > 21)
            {
                total += 1;
            }
            else
            {
                total += 11;
            }
        }

        return total;
    }

    public static string GetCardString(this List<Card> cards)
    {
        var stringBuilder = new StringBuilder();
        
        foreach (var card in cards)
        {
            stringBuilder.Append($"`{card.DisplayName}` ");
        }

        return stringBuilder.ToString();
    }
    
    public static string GetMaskedCardString(this List<Card> cards)
    {
        return $"`{cards.First().DisplayName}` `?`";
    }

    public static string GetDisplayString(this List<Card> cards)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.AppendLine($"Total: `{cards.CalculateBjTotal()}`");

        return stringBuilder.ToString();
    }
    
    public static string GetMaskedDisplayString(this List<Card> cards)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.AppendLine($"Total: `?`");

        return stringBuilder.ToString();
    }
}