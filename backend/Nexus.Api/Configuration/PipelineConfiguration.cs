using Nexus.Api.Middleware;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Configures the HTTP request pipeline (middleware order).
/// </summary>
public static class PipelineConfiguration
{
    public static WebApplication UseNexusPipeline(this WebApplication app, IConfiguration configuration)
    {
        app.UseHttpLogging();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        if (app.Environment.IsDevelopment())
            app.UseNexusSwagger(configuration);

        app.UseCors();
        app.UseAuthorization();
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        app.MapControllers();
        return app;
    }
}
