using Microsoft.EntityFrameworkCore;
using Viscoin.Bot.Features.Economy.Types;
using Viscoin.Bot.Features.Inventory.Types;
using Viscoin.Bot.Features.Lottery.Types;
using Viscoin.Bot.Features.Preconditions.Cooldown;
using Viscoin.Bot.Features.Preconditions.GamblingChannel;
using Viscoin.Bot.Features.ProvablyFair;
using Viscoin.Bot.Features.Statistics.Types;
using Viscoin.Bot.Features.User;

namespace Viscoin.Bot.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; init; } = null!;
    public DbSet<GamblingChannel> GamblingChannels { get; init; } = null!;
    public DbSet<SeedHashPair> SeedHashes { get; init; } = null!;
    public DbSet<CooldownEntity> Cooldowns { get; init; } = null!;
    public DbSet<WordleEntry> WordleEntries { get; init; } = null!;

    public DbSet<ItemEntity> ItemEntities { get; init; } = null!;
    public DbSet<PerkEntity> PerkEntities { get; init; } = null!;
    
    public DbSet<CommandExecuted> CommandsExecuted { get; init; } = null!;
    public DbSet<BalanceUpdate> BalanceUpdates { get; init; } = null!;
    
    public DbSet<Lottery> Lotteries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
}