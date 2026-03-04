namespace Nexus.Domain;

public class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.New;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
