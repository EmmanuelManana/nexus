using Nexus.Api.Configuration.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Binds options from appsettings sections (Api, Cors).
/// </summary>
public static class OptionsConfiguration
{
    public static IServiceCollection AddNexusOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));
        return services;
    }
}
