using Nexus.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNexusOptions(builder.Configuration);
builder.Services.AddNexusServices();
builder.Services.AddNexusSwagger();
builder.Services.AddNexusCors(builder.Configuration);
builder.Services.AddNexusDatabase(builder.Configuration);

builder.Logging.AddConsole();

var app = builder.Build();

app.UseNexusPipeline(app.Configuration);

// Run migrations only when argument "m" is passed (e.g. dotnet run -- m or launch profile "Nexus.Api (with migrations)")
if (args.Contains("m", StringComparer.OrdinalIgnoreCase))
    await app.RunMigrationsAsync();

await app.SeedDataAsync();

await app.RunAsync();

// Expose for WebApplicationFactory in tests
public partial class Program { }
