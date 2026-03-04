/**
 * RFC 7807 ProblemDetails shape from API.
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Array< { field?: string; message?: string } >;
}
