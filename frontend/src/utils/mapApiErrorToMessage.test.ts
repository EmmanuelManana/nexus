import { describe, it, expect } from 'vitest';
import { mapApiErrorToMessage } from './mapApiErrorToMessage';

describe('mapApiErrorToMessage', () => {
  it('returns problem.detail when present', () => {
    const err = { problem: { detail: 'Title is required.' } };
    expect(mapApiErrorToMessage(err)).toBe('Title is required.');
  });

  it('returns first validation error message when errors array is present', () => {
    const err = {
      problem: {
        errors: [
          { field: 'Title', message: 'Title is required and must be non-empty.' },
          { field: 'Status', message: 'Status must be one of: New, InProgress, Done.' },
        ],
      },
    };
    expect(mapApiErrorToMessage(err)).toContain('Title is required');
    expect(mapApiErrorToMessage(err)).toContain('and 1 more');
  });

  it('returns single validation error without "and N more"', () => {
    const err = {
      problem: {
        errors: [{ field: 'Priority', message: 'Priority must be one of: Low, Medium, High.' }],
      },
    };
    expect(mapApiErrorToMessage(err)).toBe('Priority must be one of: Low, Medium, High.');
  });

  it('returns problem.title when no detail or errors', () => {
    const err = { problem: { title: 'Validation failed', status: 400 } };
    expect(mapApiErrorToMessage(err)).toBe('Validation failed');
  });

  it('returns Error message for Error instance', () => {
    expect(mapApiErrorToMessage(new Error('Network error'))).toBe('Network error');
  });

  it('returns fallback for unknown error shape', () => {
    expect(mapApiErrorToMessage(null)).toBe('An unexpected error occurred.');
    expect(mapApiErrorToMessage(undefined)).toBe('An unexpected error occurred.');
  });
});
