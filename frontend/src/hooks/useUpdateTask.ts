import { useCallback, useState } from 'react';
import { tasksApi } from '../api/client';
import { mapApiErrorToMessage } from '../utils/mapApiErrorToMessage';
import type { Task } from '../types/task';

interface TaskFormValues {
  title: string;
  description: string;
  status: string;
  priority: string;
  dueDate: string;
}

/**
 * Custom hook for update task: submit and handle loading/error.
 */
export function useUpdateTask(id: number, onSuccess?: (task: Task) => void) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const update = useCallback(
    async (values: TaskFormValues) => {
      setLoading(true);
      setError(null);
      try {
        const task = await tasksApi.update(id, {
          title: values.title.trim(),
          description: values.description.trim(),
          status: values.status,
          priority: values.priority,
          dueDate: values.dueDate.trim() || null,
        });
        if (task) onSuccess?.(task);
        return task;
      } catch (e) {
        const msg = mapApiErrorToMessage(e);
        setError(msg);
        throw e;
      } finally {
        setLoading(false);
      }
    },
    [id, onSuccess]
  );

  return { update, loading, error, setError };
}
