using System.Security.Cryptography;
using System.Text;

namespace Viscoin.Bot.Utilities;

public static class FairRandom
{
    private static IEnumerable<byte> GetBytes(string serverseed, string clientseed, int nonce, int cursor)
    {
        int currentRound = (int)Math.Floor((double)cursor / 32);
        int currentRoundCursor = cursor;
        currentRoundCursor -= currentRound * 32;

        while (true)
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(serverseed));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{clientseed}:{nonce}:{currentRound}"));

            while (currentRoundCursor < 32)
            {
                yield return bytes[currentRoundCursor];
                currentRoundCursor += 1;
            }

            currentRoundCursor = 0;
            currentRound += 1;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private static float ByteArrayToFloat(byte[] bytes)
    {
        var result = 0f;

        for (int i = 0; i < bytes.Length; i++)
        {
            var divider = Math.Pow(256, i + 1);
            var partialResult = bytes[i] / divider;

            result += (float)partialResult;
        }

        return result;
    }

    public static float[] GetRandomFloats(string serverseed, string clientseed, int nonce, int count, int cursor = 0)
    {
        using var rng = GetBytes(serverseed, clientseed, nonce, cursor).GetEnumerator();
        List<byte> bytes = new();
        
        while (bytes.Count < count * 4)
        {
            rng.MoveNext();
            bytes.Add(rng.Current);
        }

        var floats = bytes.Chunk(4).Select(ByteArrayToFloat).ToArray();
        return floats;
    }
}