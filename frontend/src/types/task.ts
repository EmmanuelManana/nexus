/**
 * Task model matching API response.
 */
export interface Task {
  id: number;
  title: string;
  description: string;
  status: string;
  priority: string;
  dueDate: string | null;
  createdAt: string;
}

export type TaskStatus = 'New' | 'InProgress' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export const TASK_STATUSES: TaskStatus[] = ['New', 'InProgress', 'Done'];
export const TASK_PRIORITIES: TaskPriority[] = ['Low', 'Medium', 'High'];
