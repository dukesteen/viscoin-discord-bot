using Viscoin.Bot.Features.Games.Mines.Types;
using Viscoin.Bot.Features.User;
using Viscoin.Bot.Utilities;

namespace Viscoin.Bot.Features.Games.Mines;

public class MinesService
{
    public List<MinesGame> Games = new();

    public MinesGame CreateMinesGame(string serverseed, string clientseed, int nonce, int bombs, int amount,
        UserEntity player)
    {
        var game = new MinesGame
        {
            Id = Guid.NewGuid().ToString(),
            Player = player,
            Amount = amount,
            Bombs = bombs,
            Minefield = GetMineField(5, 5, bombs, serverseed, clientseed, nonce),
            Status = MinesGameStatus.Playing,
            CurrentMultiplier = 1
        };
        
        Games.Add(game);

        return game;
    }

    public MinesGame? GetMinesGameById(string gameId)
    {
        return Games.Find(x => x.Id == gameId);
    }

    public void UpdateGame(MinesGame game)
    {
        var oldGame = Games.First(x => x.Id == game.Id);
        Games.Remove(oldGame);
        Games.Add(game);
    }
    
    public double GetMinesMultiplier(MinesGame game)
    {
        double pWinning = 0d;
        double spotsLeft = 25d;
        double spotsMinusBombs = spotsLeft - game.Bombs;

        for (int i = 0; i < game.DiscoveredTiles; i++)
        {
            if (pWinning == 0)
            {
                pWinning = spotsMinusBombs / spotsLeft;
            }
            else
            {
                pWinning *= spotsMinusBombs / spotsLeft;
            }

            spotsLeft--;
            spotsMinusBombs--;
        }

        // ReSharper disable once IntDivisionByZero
        return 0.97d * (1 / pWinning);
    }
    
    private Mine[][] GetMineField(int width, int height, int bombs, string serverseed, string clientseed, int nonce)
    {
        var mineChoices = Enumerable.Range(0, 25).ToArray();

        var randomFloats = FairRandom.GetRandomFloats(serverseed, clientseed, nonce, 25);

        for (int i = mineChoices.Length - 1; i > 1; i--)
        {
            int n = (int)Math.Floor(randomFloats[i] * i);
            Swap(i, n);
        }
            
        void Swap(int x, int y)
        {
            (mineChoices[x], mineChoices[y]) = (mineChoices[y], mineChoices[x]);
        }

        Mine[][] mineField = new Mine[height][];
        for (int i = 0; i < mineField.Length; i++)
            mineField[i] = new Mine[width];

        foreach (var mineRows in mineField)
        {
            for (int j = 0; j < mineRows.Length; j++)
            {
                mineRows[j] = new Mine
                {
                    Status = MineStatus.Hidden,
                    Type = MineType.Default
                };
            }
        }

        for (int i = 0; i < bombs; i++)
        {
            var col = mineChoices[i] % width;
            var row = mineChoices[i] / height;
                
            mineField[row][col] = new Mine
            {
                Status = MineStatus.Hidden,
                Type = MineType.Bomb
            };
        }
            
        return mineField;
    }

    public void RemoveMinesGame(MinesGame game)
    {
        Games.Remove(game);
    }
}