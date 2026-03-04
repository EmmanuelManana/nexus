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
 * Custom hook for create task: submit and handle loading/error.
 */
export function useCreateTask(onSuccess?: (task: Task) => void) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const create = useCallback(
    async (values: TaskFormValues) => {
      setLoading(true);
      setError(null);
      try {
        const task = await tasksApi.create({
          title: values.title.trim(),
          description: values.description.trim(),
          status: values.status,
          priority: values.priority,
          dueDate: values.dueDate.trim() || null,
        });
        onSuccess?.(task);
        return task;
      } catch (e) {
        const msg = mapApiErrorToMessage(e);
        setError(msg);
        throw e;
      } finally {
        setLoading(false);
      }
    },
    [onSuccess]
  );

  return { create, loading, error, setError };
}
