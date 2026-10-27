using Microsoft.EntityFrameworkCore;
using MvcApp.Models;

namespace MvcApp.Data;

public static class DbSeeder
{
    /// <summary>
    /// Adds a few example tasks the first time the database is created so the
    /// ledger has something to show. Delete them at any time from the UI.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Tasks.AnyAsync())
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = DateTime.UtcNow;

        db.Tasks.AddRange(
            new TaskItem
            {
                Title = "Outline the quarterly roadmap",
                Notes = "Pull the open bets into one page. Keep it to three themes and make a call on what gets cut.",
                Category = "Product",
                Priority = TaskPriority.High,
                Status = TaskState.InProgress,
                DueDate = today.AddDays(1),
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-1)
            },
            new TaskItem
            {
                Title = "Review the new onboarding copy",
                Notes = "Read it out loud. If it sounds like a form letter, rewrite it.",
                Category = "Writing",
                Priority = TaskPriority.Medium,
                Status = TaskState.Todo,
                DueDate = today,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-2)
            },
            new TaskItem
            {
                Title = "Send the invoice for September",
                Notes = "Attach the timesheet and cc accounts.",
                Category = "Admin",
                Priority = TaskPriority.High,
                Status = TaskState.Todo,
                DueDate = today.AddDays(-2),
                CreatedAt = now.AddDays(-9),
                UpdatedAt = now.AddDays(-9)
            },
            new TaskItem
            {
                Title = "Book the dentist",
                Category = "Personal",
                Priority = TaskPriority.Low,
                Status = TaskState.Todo,
                DueDate = today.AddDays(6),
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1)
            },
            new TaskItem
            {
                Title = "Tidy the research notes into a shared doc",
                Notes = "Merge the interview highlights with the survey readout.",
                Category = "Product",
                Priority = TaskPriority.Medium,
                Status = TaskState.Todo,
                DueDate = null,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5)
            },
            new TaskItem
            {
                Title = "Set up the staging environment",
                Category = "Engineering",
                Priority = TaskPriority.High,
                Status = TaskState.Done,
                DueDate = today.AddDays(-1),
                CreatedAt = now.AddDays(-7),
                UpdatedAt = now.AddDays(-1),
                CompletedAt = now.AddDays(-1)
            },
            new TaskItem
            {
                Title = "Pick a name for the side project",
                Category = "Personal",
                Priority = TaskPriority.Low,
                Status = TaskState.Done,
                CreatedAt = now.AddDays(-12),
                UpdatedAt = now.AddDays(-4),
                CompletedAt = now.AddDays(-4)
            });

        await db.SaveChangesAsync();
    }
}
