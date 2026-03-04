using Nexus.Domain;

namespace Nexus.Application.Dtos;

public record TaskResponse(
    int Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    string? DueDate,
    string CreatedAt
);

public static class TaskResponseMapper
{
    public static TaskResponse FromEntity(TaskItem entity) => new(
        entity.Id,
        entity.Title,
        entity.Description,
        entity.Status.ToString(),
        entity.Priority.ToString(),
        entity.DueDate.HasValue ? entity.DueDate.Value.ToString("O") : null,
        entity.CreatedAt.ToString("O")
    );
}
