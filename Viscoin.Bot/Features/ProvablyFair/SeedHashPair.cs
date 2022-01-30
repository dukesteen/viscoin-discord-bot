using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viscoin.Bot.Features.ProvablyFair;

public class SeedHashPair
{
    public SeedHashPair(string serverSeed, string hash)
    {
        ServerSeed = serverSeed;
        Hash = hash;
    }
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string ServerSeed { get; set; }
    public string Hash { get; set; }
}