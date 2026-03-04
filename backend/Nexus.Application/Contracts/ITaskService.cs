using Nexus.Application.Dtos;
using Nexus.Domain;

namespace Nexus.Application.Contracts;

/// <summary>
/// Application service for task operations (Single Responsibility).
/// </summary>
public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> GetTasksAsync(string? searchQuery, bool sortDueDateAsc, CancellationToken cancellationToken = default);
    Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
