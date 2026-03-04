using Nexus.Api.Configuration.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configures CORS from the Cors section in appsettings.
/// </summary>
public static class CorsConfiguration
{
    public static IServiceCollection AddNexusCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (corsOptions.AllowAnyOrigin)
                    policy.AllowAnyOrigin();
                else if (corsOptions.AllowedOrigins?.Length > 0)
                    policy.WithOrigins(corsOptions.AllowedOrigins).AllowAnyMethod().AllowAnyHeader();
            });
        });
        return services;
    }
}
