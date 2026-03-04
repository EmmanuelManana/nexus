import { config } from '../config/api';

const baseUrl = config.apiBaseUrl;

async function handleResponse<T>(response: Response): Promise<T> {
  const contentType = response.headers.get('content-type') ?? '';
  const isJson = contentType.includes('application/json') || contentType.includes('application/problem+json');

  if (!response.ok) {
    if (isJson) {
      const body = await response.json();
      throw { status: response.status, problem: body };
    }
    throw { status: response.status, problem: { detail: response.statusText } };
  }

  if (response.status === 204) return undefined as T;
  if (!isJson) return undefined as T;
  return response.json();
}

export const tasksApi = {
  async getAll(params: { q?: string; sort?: string }): Promise<import('../types/task').Task[]> {
    const search = new URLSearchParams();
    if (params.q) search.set('q', params.q);
    if (params.sort) search.set('sort', params.sort);
    const url = `${baseUrl}/api/tasks${search.toString() ? `?${search}` : ''}`;
    const res = await fetch(url);
    return handleResponse(res);
  },

  async getById(id: number): Promise<import('../types/task').Task | null> {
    const res = await fetch(`${baseUrl}/api/tasks/${id}`);
    if (res.status === 404) return null;
    return handleResponse(res);
  },

  async create(body: {
    title: string;
    description: string;
    status: string;
    priority: string;
    dueDate?: string | null;
  }): Promise<import('../types/task').Task> {
    const res = await fetch(`${baseUrl}/api/tasks`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    return handleResponse(res);
  },

  async update(
    id: number,
    body: {
      title: string;
      description: string;
      status: string;
      priority: string;
      dueDate?: string | null;
    }
  ): Promise<import('../types/task').Task | null> {
    const res = await fetch(`${baseUrl}/api/tasks/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    if (res.status === 404) return null;
    return handleResponse(res);
  },

  async delete(id: number): Promise<boolean> {
    const res = await fetch(`${baseUrl}/api/tasks/${id}`, { method: 'DELETE' });
    if (res.status === 404) return false;
    return true;
  },
};
