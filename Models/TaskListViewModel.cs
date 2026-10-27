namespace MvcApp.Models;

public sealed record TaskSummary(
    int Total,
    int Todo,
    int InProgress,
    int Done,
    int DueToday,
    int Overdue)
{
    public int Open => Total - Done;

    public int CompletionPercent =>
        Total == 0 ? 0 : (int)Math.Round(Done * 100d / Total);
}

public sealed class TaskListViewModel
{
    public IReadOnlyList<TaskItem> Tasks { get; init; } = [];

    public IReadOnlyList<string> Categories { get; init; } = [];

    public TaskSummary Summary { get; init; } = new(0, 0, 0, 0, 0, 0);

    public string? Search { get; init; }

    public TaskState? Status { get; init; }

    public TaskPriority? Priority { get; init; }

    public string? Category { get; init; }

    public string Sort { get; init; } = "due";

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Search) ||
        Status is not null ||
        Priority is not null ||
        !string.IsNullOrWhiteSpace(Category);
}
