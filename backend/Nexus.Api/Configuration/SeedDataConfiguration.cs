using Microsoft.EntityFrameworkCore;
using Nexus.Domain;
using Nexus.Infrastructure.Persistence;

namespace Nexus.Api.Configuration;

/// <summary>
/// Seeds initial task data when the database is empty.
/// </summary>
public static class SeedDataConfiguration
{
    public static async Task SeedDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (await db.Tasks.AnyAsync()) return;

        var utc = DateTime.UtcNow;
        var tasks = new[]
        {
            new TaskItem
            {
                Title = "Design API contract",
                Description = "Define REST endpoints and request/response shapes for the Task Tracker API.",
                Status = Domain.TaskStatus.Done,
                Priority = TaskPriority.High,
                DueDate = utc.AddDays(-2),
                CreatedAt = utc.AddDays(-5)
            },
            new TaskItem
            {
                Title = "Implement task list view",
                Description = "Build the React task list with search and sort.",
                Status = Domain.TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = utc.AddDays(1),
                CreatedAt = utc.AddDays(-3)
            },
            new TaskItem
            {
                Title = "Write unit tests",
                Description = "Add xUnit tests for API and frontend utilities.",
                Status = Domain.TaskStatus.New,
                Priority = TaskPriority.Medium,
                DueDate = utc.AddDays(3),
                CreatedAt = utc.AddDays(-1)
            },
            new TaskItem
            {
                Title = "Document SOLUTION.md",
                Description = "Describe design decisions and debugging notes.",
                Status = Domain.TaskStatus.New,
                Priority = TaskPriority.Low,
                DueDate = null,
                CreatedAt = utc
            }
        };

        await db.Tasks.AddRangeAsync(tasks);
        await db.SaveChangesAsync();
    }
}
