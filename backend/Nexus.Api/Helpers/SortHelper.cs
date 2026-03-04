namespace Nexus.Api.Helpers;

/// <summary>
/// Parses task list query parameters (e.g. sort=dueDate:asc|desc).
/// </summary>
public static class SortHelper
{
    /// <summary>
    /// Parses the sort query value. Returns true for dueDate:asc (default), false for dueDate:desc.
    /// </summary>
    public static bool ParseDueDateSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return true; // default dueDate:asc
        var parts = sort.Trim().Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return true;
        if (!parts[0].Trim().Equals("dueDate", StringComparison.OrdinalIgnoreCase)) return true;
        return parts[1].Trim().Equals("asc", StringComparison.OrdinalIgnoreCase);
    }
}
