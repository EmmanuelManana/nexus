import { Link } from 'react-router-dom';
import { useTasksContext } from '../context/TasksContext';
import type { Task } from '../types/task';

function formatDate(iso: string | null): string {
  if (!iso) return '—';
  try {
    return new Date(iso).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  } catch {
    return iso;
  }
}

export function TaskList() {
  const { tasks, loading, error, sortDueDateAsc, setSortDueDateAsc, searchQuery, setSearchQuery } =
    useTasksContext();

  return (
    <section className="task-list">
      <h2>Tasks</h2>
      <div className="list-controls">
        <input
          type="search"
          placeholder="Search title or description..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          aria-label="Search tasks"
          className="search-input"
        />
        <label className="sort-control">
          <span>Sort by due date:</span>
          <select
            value={sortDueDateAsc ? 'asc' : 'desc'}
            onChange={(e) => setSortDueDateAsc(e.target.value === 'asc')}
            aria-label="Sort by due date"
          >
            <option value="asc">Ascending</option>
            <option value="desc">Descending</option>
          </select>
        </label>
      </div>

      {error && (
        <div className="error-banner" role="alert">
          {error}
        </div>
      )}

      {loading ? (
        <p className="loading">Loading tasks…</p>
      ) : (
        <ul className="tasks">
          {tasks.map((t) => (
            <TaskRow key={t.id} task={t} />
          ))}
        </ul>
      )}
      {!loading && tasks.length === 0 && (
        <p className="empty">No tasks found. Create one to get started.</p>
      )}
      <p className="actions">
        <Link to="/tasks/new" className="btn btn-primary">
          New task
        </Link>
      </p>
    </section>
  );
}

function TaskRow({ task }: { task: Task }) {
  return (
    <li className="task-row">
      <Link to={`/tasks/${task.id}/edit`} className="task-link">
        <span className="task-title">{task.title}</span>
        <span className="task-meta">
          {task.status} · {task.priority}
          {task.dueDate && ` · Due ${formatDate(task.dueDate)}`}
        </span>
      </Link>
    </li>
  );
}
