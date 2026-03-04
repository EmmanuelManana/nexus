using Microsoft.EntityFrameworkCore;
using Nexus.Domain;

namespace Nexus.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(500);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.Priority).HasConversion<string>();
        });
    }
}
