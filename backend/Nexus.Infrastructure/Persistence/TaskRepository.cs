using Microsoft.EntityFrameworkCore;
using Nexus.Application.Contracts;
using Nexus.Domain;

namespace Nexus.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of task repository with Strategy for sort order (Open/Closed).
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(string? searchQuery, bool sortDueDateAsc, CancellationToken cancellationToken = default)
    {
        var query = _db.Tasks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var q = searchQuery.Trim().ToLowerInvariant();
            query = query.Where(t =>
                (t.Title != null && t.Title.ToLower().Contains(q)) ||
                (t.Description != null && t.Description.ToLower().Contains(q)));
        }

        query = sortDueDateAsc
            ? query.OrderBy(t => t.DueDate == null).ThenBy(t => t.DueDate)
            : query.OrderBy(t => t.DueDate == null).ThenByDescending(t => t.DueDate);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<TaskItem> AddAsync(TaskItem entity, CancellationToken cancellationToken = default)
    {
        _db.Tasks.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TaskItem?> UpdateAsync(int id, Action<TaskItem> update, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Tasks.FindAsync([id], cancellationToken);
        if (entity is null) return null;
        update(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Tasks.FindAsync([id], cancellationToken);
        if (entity is null) return false;
        _db.Tasks.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Tasks.AnyAsync(t => t.Id == id, cancellationToken);
}
