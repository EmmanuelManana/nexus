using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nexus.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core tools (e.g. migrations) when DbContext lives in Infrastructure.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseInMemoryDatabase("NexusTasks");
        return new AppDbContext(optionsBuilder.Options);
    }
}
