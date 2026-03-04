import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { TaskForm, type TaskFormValues } from '../components/TaskForm';
import { useTask } from '../hooks/useTask';
import { useUpdateTask } from '../hooks/useUpdateTask';
import { useTasksContext } from '../context/TasksContext';

export function EditTaskPage() {
  const { id } = useParams<{ id: string }>();
  const taskId = id != null ? parseInt(id, 10) : null;
  const isInvalidId = taskId != null && (Number.isNaN(taskId) || taskId < 1);
  const numericId = taskId != null && !Number.isNaN(taskId) ? taskId : null;
  const { task, loading: loadingTask, error: loadError, refetch } = useTask(numericId);
  const { refetch: refetchList } = useTasksContext();
  const { update, loading: saving, error: saveError } = useUpdateTask(numericId ?? 0, () => {
    refetchList();
    refetch();
  });

  useEffect(() => {
    if (numericId != null) refetch();
  }, [numericId, refetch]);

  if (isInvalidId) {
    return (
      <p>
        Invalid task id. <a href="/">Back to list</a>
      </p>
    );
  }

  if (loadingTask) return <p className="loading">Loading task…</p>;
  if (loadError) return <p className="error-banner">{loadError}</p>;
  if (!task) return <p>Task not found. <a href="/">Back to list</a></p>;

  const initialValues: TaskFormValues = {
    title: task.title,
    description: task.description,
    status: task.status,
    priority: task.priority,
    dueDate: task.dueDate ?? '',
  };

  return (
    <>
      <h1>Edit task</h1>
      <TaskForm
        initialValues={initialValues}
        submitLabel="Save"
        loading={saving}
        error={saveError}
        onSubmit={async (values) => {
          await update(values);
        }}
      />
    </>
  );
}
