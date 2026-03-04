import type { ProblemDetails } from '../types/problemDetails';

/**
 * Maps API error (ProblemDetails) to a user-friendly message.
 * Used by useApiError and for displaying validation/error state in UI.
 */
export function mapApiErrorToMessage(error: unknown): string {
  if (error && typeof error === 'object' && 'problem' in error) {
    const problem = (error as { problem?: ProblemDetails }).problem;
    if (problem?.detail) return problem.detail;
    if (problem?.errors && Array.isArray(problem.errors) && problem.errors.length > 0) {
      const first = problem.errors[0];
      const msg = first?.message ?? first?.field ?? 'Validation failed';
      if (problem.errors.length > 1) return `${msg} (and ${problem.errors.length - 1} more)`;
      return typeof msg === 'string' ? msg : 'Validation failed';
    }
    if (problem?.title) return problem.title;
  }
  if (error instanceof Error) return error.message;
  return 'An unexpected error occurred.';
}
