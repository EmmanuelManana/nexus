import { useCallback, useState } from 'react';
import { mapApiErrorToMessage } from '../utils/mapApiErrorToMessage';

/**
 * Custom hook that maps API errors to user-friendly messages.
 * Used when calling create/update/delete and displaying errors in the UI.
 */
export function useApiError() {
  const [error, setError] = useState<string | null>(null);

  const setErrorFrom = useCallback((e: unknown) => {
    setError(mapApiErrorToMessage(e));
  }, []);

  const clearError = useCallback(() => setError(null), []);

  return { error, setErrorFrom, clearError };
}
