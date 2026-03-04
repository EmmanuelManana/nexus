namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configures MVC controllers and HTTP logging.
/// </summary>
public static class ServiceConfiguration
{
    public static IServiceCollection AddNexusServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        });
        return services;
    }
}
