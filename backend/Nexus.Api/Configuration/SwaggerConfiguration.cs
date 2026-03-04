using Microsoft.OpenApi.Models;
using Nexus.Api.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configures Swagger/OpenAPI services.
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddNexusSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nexus Task Tracker API", Version = "v1" });
        });
        return services;
    }
}
