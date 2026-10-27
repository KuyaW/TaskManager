using System.ComponentModel.DataAnnotations;

namespace MvcApp.Models;

public enum TaskState
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}

public class TaskItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Give the task a title.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "The title must be between 3 and 120 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2,000 characters.")]
    public string? Notes { get; set; }

    [StringLength(50, ErrorMessage = "The category cannot exceed 50 characters.")]
    public string? Category { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public TaskState Status { get; set; } = TaskState.Todo;

    [DataType(DataType.Date)]
    public DateOnly? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public bool IsDone => Status == TaskState.Done;

    public bool IsOverdue =>
        !IsDone && DueDate is { } due && due < DateOnly.FromDateTime(DateTime.Today);

    public bool IsDueToday =>
        !IsDone && DueDate is { } due && due == DateOnly.FromDateTime(DateTime.Today);

    public string StatusLabel => Status switch
    {
        TaskState.InProgress => "In progress",
        TaskState.Done => "Done",
        _ => "To do"
    };

    public string PriorityLabel => Priority.ToString();
}
