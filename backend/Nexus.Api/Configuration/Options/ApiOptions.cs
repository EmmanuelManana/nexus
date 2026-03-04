namespace Nexus.Api.Configuration.Options;

/// <summary>
/// API section in appsettings.json.
/// </summary>
public class ApiOptions
{
    public const string SectionName = "Api";

    public string Name { get; set; } = "Nexus Task Tracker API";
    public bool SwaggerEnabled { get; set; } = true;
}
