using System.Globalization;
using Nexus.Application.Dtos;

namespace Nexus.Api.Validation;

/// <summary>
/// Validates task request DTOs (Single Responsibility). Returns validation errors for ProblemDetails.
/// </summary>
public static class TaskRequestValidator
{
    private static readonly string[] ValidStatuses = { "new", "inprogress", "done" };
    private static readonly string[] ValidPriorities = { "low", "medium", "high" };

    public static IReadOnlyList<ValidationError> ValidateCreate(CreateTaskRequest request)
    {
        var errors = new List<ValidationError>();
        ValidateTitle(request.Title, errors);
        ValidateStatus(request.Status, errors);
        ValidatePriority(request.Priority, errors);
        ValidateDueDate(request.DueDate, errors);
        return errors;
    }

    public static IReadOnlyList<ValidationError> ValidateUpdate(UpdateTaskRequest request)
    {
        var errors = new List<ValidationError>();
        ValidateTitle(request.Title, errors);
        ValidateStatus(request.Status, errors);
        ValidatePriority(request.Priority, errors);
        ValidateDueDate(request.DueDate, errors);
        return errors;
    }

    private static void ValidateTitle(string? title, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add(new ValidationError("Title", "Title is required and must be non-empty."));
            return;
        }
        if (title.Length > 500)
            errors.Add(new ValidationError("Title", "Title must not exceed 500 characters."));
    }

    private static void ValidateStatus(string? status, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            errors.Add(new ValidationError("Status", "Status is required. Must be one of: New, InProgress, Done."));
            return;
        }
        var normalized = status.Trim().ToLowerInvariant().Replace(" ", "");
        if (!ValidStatuses.Contains(normalized))
            errors.Add(new ValidationError("Status", "Status must be one of: New, InProgress, Done."));
    }

    private static void ValidatePriority(string? priority, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(priority))
        {
            errors.Add(new ValidationError("Priority", "Priority is required. Must be one of: Low, Medium, High."));
            return;
        }
        var normalized = priority.Trim().ToLowerInvariant();
        if (!ValidPriorities.Contains(normalized))
            errors.Add(new ValidationError("Priority", "Priority must be one of: Low, Medium, High."));
    }

    private static void ValidateDueDate(string? dueDate, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(dueDate)) return;
        if (!DateTime.TryParse(dueDate, null, DateTimeStyles.RoundtripKind, out _))
            errors.Add(new ValidationError("DueDate", "DueDate, if provided, must be a valid ISO-8601 date."));
    }
}

public record ValidationError(string Field, string Message);
