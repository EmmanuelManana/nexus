import { TASK_PRIORITIES, TASK_STATUSES } from '../types/task';

export interface TaskFormValues {
  title: string;
  description: string;
  status: string;
  priority: string;
  dueDate: string;
}

interface TaskFormProps {
  initialValues?: Partial<TaskFormValues>;
  onSubmit: (values: TaskFormValues) => void | Promise<void>;
  loading?: boolean;
  error?: string | null;
  submitLabel: string;
}

const defaultValues: TaskFormValues = {
  title: '',
  description: '',
  status: 'New',
  priority: 'Medium',
  dueDate: '',
};

export function TaskForm({
  initialValues,
  onSubmit,
  loading = false,
  error,
  submitLabel,
}: TaskFormProps) {
  const values = { ...defaultValues, ...initialValues };

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = e.currentTarget;
    const dueRaw = (form.elements.namedItem('dueDate') as HTMLInputElement).value;
    const data: TaskFormValues = {
      title: (form.elements.namedItem('title') as HTMLInputElement).value,
      description: (form.elements.namedItem('description') as HTMLTextAreaElement).value,
      status: (form.elements.namedItem('status') as HTMLSelectElement).value,
      priority: (form.elements.namedItem('priority') as HTMLSelectElement).value,
      dueDate: dueRaw ? new Date(dueRaw).toISOString() : '',
    };
    void onSubmit(data);
  };

  return (
    <form onSubmit={handleSubmit} className="task-form">
      {error && (
        <div className="error-banner" role="alert">
          {error}
        </div>
      )}
      <div className="form-group">
        <label htmlFor="title">Title *</label>
        <input
          id="title"
          name="title"
          type="text"
          required
          defaultValue={values.title}
          placeholder="Task title"
          maxLength={500}
          aria-required="true"
        />
      </div>
      <div className="form-group">
        <label htmlFor="description">Description</label>
        <textarea
          id="description"
          name="description"
          rows={3}
          defaultValue={values.description}
          placeholder="Optional description"
        />
      </div>
      <div className="form-row">
        <div className="form-group">
          <label htmlFor="status">Status *</label>
          <select id="status" name="status" required aria-required="true" defaultValue={values.status}>
            {TASK_STATUSES.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </div>
        <div className="form-group">
          <label htmlFor="priority">Priority *</label>
          <select id="priority" name="priority" required aria-required="true" defaultValue={values.priority}>
            {TASK_PRIORITIES.map((p) => (
              <option key={p} value={p}>
                {p}
              </option>
            ))}
          </select>
        </div>
      </div>
      <div className="form-group">
        <label htmlFor="dueDate">Due date (ISO-8601)</label>
        <input
          id="dueDate"
          name="dueDate"
          type="datetime-local"
          defaultValue={values.dueDate ? values.dueDate.slice(0, 16).replace('Z', '') : ''}
          aria-description="Optional; use ISO-8601 format if pasting"
        />
      </div>
      <div className="form-actions">
        <button type="submit" disabled={loading} className="btn btn-primary">
          {loading ? 'Saving…' : submitLabel}
        </button>
      </div>
    </form>
  );
}
