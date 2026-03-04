using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Nexus.Application.Dtos;
using Xunit;

namespace Nexus.Api.Tests;

/// <summary>
/// Validation/error test: invalid POST returns 400 with ProblemDetails.
/// </summary>
public class InvalidPostReturnsProblemDetailsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InvalidPostReturnsProblemDetailsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_With_Empty_Title_Returns_400_With_ProblemDetails()
    {
        var request = new CreateTaskRequest(
            Title: "",
            Description: "Some description",
            Status: "New",
            Priority: "Medium",
            DueDate: null
        );

        var response = await _client.PostAsJsonAsync("/api/tasks", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
        Assert.True(contentType.Contains("problem+json") || contentType.Contains("application/json"), "Expected problem+json or application/json");

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal(400, root.GetProperty("status").GetInt32());
        Assert.True(root.TryGetProperty("title", out _));
        Assert.True(root.TryGetProperty("detail", out _));
        Assert.True(root.TryGetProperty("errors", out var errors));
        Assert.True(errors.GetArrayLength() > 0);
    }

    [Fact]
    public async Task Post_With_Invalid_Status_Returns_400()
    {
        var request = new CreateTaskRequest(
            Title: "Valid title",
            Description: "",
            Status: "InvalidStatus",
            Priority: "High",
            DueDate: null
        );

        var response = await _client.PostAsJsonAsync("/api/tasks", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
