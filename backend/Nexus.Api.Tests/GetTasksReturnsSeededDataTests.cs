using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Nexus.Application.Dtos;
using Xunit;

namespace Nexus.Api.Tests;

/// <summary>
/// Happy path: GET /api/tasks returns seeded data.
/// </summary>
public class GetTasksReturnsSeededDataTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GetTasksReturnsSeededDataTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTasks_Returns_Ok_With_Seeded_Tasks()
    {
        var response = await _client.GetAsync("/api/tasks");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var tasks = JsonSerializer.Deserialize<List<TaskResponse>>(json, options);
        Assert.NotNull(tasks);
        Assert.True(tasks.Count >= 4, "Expected at least 4 seeded tasks.");
        Assert.Contains(tasks, t => t.Title.Contains("Design API", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetTasks_With_Sort_Returns_Ok()
    {
        var response = await _client.GetAsync("/api/tasks?sort=dueDate:desc");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var tasks = JsonSerializer.Deserialize<List<TaskResponse>>(json, options);
        Assert.NotNull(tasks);
    }
}
