namespace Nexus.Api.Configuration.Options;

/// <summary>
/// CORS section in appsettings.json.
/// </summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool AllowAnyOrigin { get; set; }
}
