using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Viscoin.Bot.Infrastructure.Config;

namespace Viscoin.Bot.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ViscoinConfig.Configuration.ApplicationDbContext);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}