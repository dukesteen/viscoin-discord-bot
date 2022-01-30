namespace Viscoin.Bot.Features.Games.Dice;

public class DiceService
{
    public double GetMultiplier(DicePicks choice, double pick)
        {
            double winChance = 0;

            switch (choice)
            {
                case DicePicks.Under:
                    winChance = pick / 100d;
                    break;
                case DicePicks.Over:
                    winChance = (100d - pick) / 100d;
                    break;
            }

            var multiplier = Math.Round(99 / (winChance * 100), 3);
            return multiplier;
        }

        public int GetWinnings(DicePicks choice, double dicePick, double pick, double multiplier, int betAmount)
        {
            switch (choice)
            {
                case DicePicks.Over:
                    if (dicePick > pick)
                    {
                        var winnings = Math.Round((betAmount * multiplier) - betAmount);
                        if (winnings == 0)
                        {
                            return 0;
                        }
                        return (int)winnings;
                    }
                    else
                    {
                        return -betAmount;
                    }
                case DicePicks.Under:
                    if (dicePick < pick)
                    {
                        var winnings = Math.Round((betAmount * multiplier) - betAmount);
                        if (winnings == 0)
                        {
                            return 0;
                        }

                        return (int)winnings;
                    }
                    else
                    {
                        return -betAmount;
                    }
                default:
                    throw new Exception("Er is iets mis gegaan");
            }
        }
}