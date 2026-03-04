namespace Nexus.Application.Dtos;

public record CreateTaskRequest(
    string Title,
    string Description,
    string Status,
    string Priority,
    string? DueDate
);

public record UpdateTaskRequest(
    string Title,
    string Description,
    string Status,
    string Priority,
    string? DueDate
);
