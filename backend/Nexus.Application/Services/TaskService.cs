using Nexus.Application.Contracts;
using Nexus.Application.Dtos;
using Nexus.Domain;

namespace Nexus.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TaskResponse>> GetTasksAsync(string? searchQuery, bool sortDueDateAsc, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(searchQuery, sortDueDateAsc, cancellationToken);
        return entities.Select(TaskResponseMapper.FromEntity).ToList();
    }

    public async Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : TaskResponseMapper.FromEntity(entity);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Status = ParseStatus(request.Status),
            Priority = ParsePriority(request.Priority),
            DueDate = ParseDueDate(request.DueDate),
            CreatedAt = DateTime.UtcNow
        };
        var created = await _repository.AddAsync(entity, cancellationToken);
        return TaskResponseMapper.FromEntity(created);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateAsync(id, entity =>
        {
            entity.Title = request.Title.Trim();
            entity.Description = request.Description?.Trim() ?? string.Empty;
            entity.Status = ParseStatus(request.Status);
            entity.Priority = ParsePriority(request.Priority);
            entity.DueDate = ParseDueDate(request.DueDate);
        }, cancellationToken);
        return updated is null ? null : TaskResponseMapper.FromEntity(updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        await _repository.DeleteAsync(id, cancellationToken);

    private static Domain.TaskStatus ParseStatus(string value) => value?.ToLowerInvariant() switch
    {
        "new" => Domain.TaskStatus.New,
        "inprogress" => Domain.TaskStatus.InProgress,
        "done" => Domain.TaskStatus.Done,
        _ => Domain.TaskStatus.New
    };

    private static TaskPriority ParsePriority(string value) => value?.ToLowerInvariant() switch
    {
        "low" => TaskPriority.Low,
        "medium" => TaskPriority.Medium,
        "high" => TaskPriority.High,
        _ => TaskPriority.Medium
    };

    private static DateTime? ParseDueDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d) ? d : null;
    }
}
