using Nexus.Domain;

namespace Nexus.Application.Contracts;

/// <summary>
/// Repository abstraction for tasks (Dependency Inversion).
/// </summary>
public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(string? searchQuery, bool sortDueDateAsc, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskItem> AddAsync(TaskItem entity, CancellationToken cancellationToken = default);
    Task<TaskItem?> UpdateAsync(int id, Action<TaskItem> update, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
