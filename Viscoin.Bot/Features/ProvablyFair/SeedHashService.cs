using System.Security.Cryptography;
using System.Text;
using Viscoin.Bot.Infrastructure.Data;

namespace Viscoin.Bot.Features.ProvablyFair;

public class SeedHashService
{
    private readonly ApplicationDbContext _db;

    public SeedHashService(ApplicationDbContext db)
    {
        _db = db;
    }
    
    public async Task AddSeed(string seed)
    {
        string hash;
        using (SHA256 hasher = SHA256.Create())
        {
            hash = Convert.ToHexString(hasher.ComputeHash(Encoding.UTF8.GetBytes(seed)));
        }

        _db.SeedHashes.Add(new SeedHashPair(seed, hash));

        await _db.SaveChangesAsync();
    }

    public async Task<SeedHashPair?> GetPair(string seed)
    {
        return await _db.SeedHashes.FindAsync(seed);
    }
}