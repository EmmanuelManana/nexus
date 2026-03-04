using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Validation;

namespace Nexus.Api.Helpers;

/// <summary>
/// Builds RFC 7807 ProblemDetails for API error responses.
/// </summary>
public static class ProblemDetailsHelper
{
    public static ProblemDetails For(int status, string title, string detail, int? id = null)
    {
        var p = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = title,
            Status = status,
            Detail = detail
        };
        if (id.HasValue)
            p.Extensions["id"] = id.Value;
        return p;
    }

    public static ProblemDetails ForValidation(int status, string title, IReadOnlyList<ValidationError> errors)
    {
        var p = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = title,
            Status = status,
            Detail = "One or more validation errors occurred."
        };
        p.Extensions["errors"] = errors.Select(e => new { e.Field, e.Message }).ToList();
        return p;
    }
}
