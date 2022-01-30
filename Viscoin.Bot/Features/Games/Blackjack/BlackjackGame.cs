using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared.Types;

namespace Viscoin.Bot.Features.Games.Blackjack;

public class BlackjackGame
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public Deck Deck { get; set; } = null!;
    public int Amount { get; set; }
    public UserEntity Player { get; set; } = null!;
    public List<Card> PlayerCards { get; set; } = new();
    public List<Card> DealerCards { get; set; } = new();
}