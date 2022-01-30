namespace Viscoin.Bot.Shared.Types;

public class Deck
{

    public List<Card> Cards { get; set; } = new();
    
    public Deck()
    {
        foreach (var rank in AppConstants.Ranks)
        {
            foreach (var suit in AppConstants.Suits)
            {
                Cards.Add(new Card(rank, suit, suit + rank));
            }
        }
    }

    public Card Next()
    {
        var card = Cards.First();
        Cards.Remove(card);

        return card;
    }
}

public static class DeckExtensions
{
    public static void Shuffle(this List<Card> cards, float[] randomFloats)
    {
        for (int i = cards.Count - 1; i > 1; i--)
        {
            int n = (int)Math.Floor(randomFloats[i] * i);
            Swap(i, n);
        }
            
        void Swap(int x, int y)
        {
            (cards[x], cards[y]) = (cards[y], cards[x]);
        }
    }
}

public class Card
{
    public Card(string rank, char suit, string displayName)
    {
        Rank = rank;
        Suit = suit;
        DisplayName = displayName;
    }

    public string Rank { get; init; }
    public char Suit { get; set; }
    public string DisplayName { get; init; }
}