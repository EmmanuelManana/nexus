using Microsoft.EntityFrameworkCore;
using Nexus.Infrastructure.Persistence;

namespace Nexus.Api.Configuration;

/// <summary>
/// Applies pending EF Core migrations at startup when requested (e.g. via argument "m").
/// Only runs when the database provider is relational (SQLite, SQL Server, etc.); no-op for InMemory.
/// </summary>
public static class MigrationConfiguration
{
    public static async Task RunMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();
    }
}
