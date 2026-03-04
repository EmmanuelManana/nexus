import { useCallback, useState } from 'react';
import { tasksApi } from '../api/client';
import { mapApiErrorToMessage } from '../utils/mapApiErrorToMessage';
import type { Task } from '../types/task';

interface UseTaskOptions {
  onSuccess?: () => void;
  onError?: (message: string) => void;
}

/**
 * Custom hook for fetching a single task by id (e.g. for edit page).
 */
export function useTask(id: number | null, options?: UseTaskOptions) {
  const [task, setTask] = useState<Task | null>(null);
  const [loading, setLoading] = useState(!!id);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    if (id == null) {
      setTask(null);
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const data = await tasksApi.getById(id);
      setTask(data);
    } catch (e) {
      const msg = mapApiErrorToMessage(e);
      setError(msg);
      options?.onError?.(msg);
    } finally {
      setLoading(false);
    }
  }, [id, options]);

  return { task, loading, error, refetch };
}
