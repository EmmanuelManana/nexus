/**
 * API base URL from environment. Vite exposes env vars prefixed with VITE_.
 */
export const config = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
} as const;
