using Nexus.Api.Configuration.Options;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Configures Swagger middleware when enabled in options.
/// </summary>
public static class SwaggerPipelineConfiguration
{
    public static IApplicationBuilder UseNexusSwagger(this IApplicationBuilder app, IConfiguration configuration)
    {
        var apiOpts = configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>();
        if (apiOpts?.SwaggerEnabled == true)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        return app;
    }
}
