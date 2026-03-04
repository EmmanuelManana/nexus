using Microsoft.EntityFrameworkCore;
using Nexus.Application.Contracts;
using Nexus.Application.Services;
using Nexus.Infrastructure.Persistence;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configures EF Core DbContext, repository, and application services.
/// </summary>
public static class DatabaseConfiguration
{
    public static IServiceCollection AddNexusDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseInMemoryDatabase("NexusTasks");
        });

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
