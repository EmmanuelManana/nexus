import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from 'react';
import { tasksApi } from '../api/client';
import type { Task } from '../types/task';

interface TasksState {
  tasks: Task[];
  loading: boolean;
  error: string | null;
  sortDueDateAsc: boolean;
  searchQuery: string;
}

interface TasksContextValue extends TasksState {
  refetch: () => Promise<void>;
  setSortDueDateAsc: (asc: boolean) => void;
  setSearchQuery: (q: string) => void;
}

const TasksContext = createContext<TasksContextValue | null>(null);

export function TasksProvider({ children }: { children: ReactNode }) {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sortDueDateAsc, setSortDueDateAsc] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');

  const refetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const sort = sortDueDateAsc ? 'dueDate:asc' : 'dueDate:desc';
      const data = await tasksApi.getAll({ q: searchQuery || undefined, sort });
      setTasks(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load tasks');
    } finally {
      setLoading(false);
    }
  }, [sortDueDateAsc, searchQuery]);

  useEffect(() => {
    refetch();
  }, [refetch]);

  const value: TasksContextValue = {
    tasks,
    loading,
    error,
    sortDueDateAsc,
    searchQuery,
    refetch,
    setSortDueDateAsc,
    setSearchQuery,
  };

  return <TasksContext.Provider value={value}>{children}</TasksContext.Provider>;
}

export function useTasksContext() {
  const ctx = useContext(TasksContext);
  if (!ctx) throw new Error('useTasksContext must be used within TasksProvider');
  return ctx;
}
