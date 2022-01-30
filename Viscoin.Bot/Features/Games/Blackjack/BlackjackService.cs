using Viscoin.Bot.Features.User;
using Viscoin.Bot.Shared.Types;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Games.Blackjack;

public class BlackjackService
{
    private List<BlackjackGame> BlackjackGames { get; set; } = new();
    
    public BlackjackGame StartGame(string serverseed, string clientseed, int nonce, int amount, UserEntity user, int shuffles)
    {
        var game = new BlackjackGame
        {
            Amount = amount,
            Player = user
        };
        var deck = new Deck();
            
        for (int i = 0; i < shuffles; i++)
        {
            deck.Cards.Shuffle(FairRandom.GetRandomFloats(serverseed, clientseed, nonce, 52));
        }

        game.Deck = deck;

        game.DealerCards.Add(deck.Next());
        game.DealerCards.Add(deck.Next());
            
        game.PlayerCards.Add(deck.Next());
        game.PlayerCards.Add(deck.Next());
            
        BlackjackGames.Add(game);

        return game;
    }

    public void UpdateGame(BlackjackGame game)
    {
        var prev = BlackjackGames.First(x => x.Id == game.Id);
        BlackjackGames.Remove(prev);
        BlackjackGames.Add(game);
    }

    public void RemoveGame(BlackjackGame game)
    {
        BlackjackGames.Remove(game);
    }

    public BlackjackGame? GetGame(string gameId)
    {
        return BlackjackGames.FirstOrDefault(x => x.Id == gameId);
    }
}