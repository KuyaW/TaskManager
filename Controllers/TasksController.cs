using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcApp.Data;
using MvcApp.Models;

namespace MvcApp.Controllers;

public class TasksController(AppDbContext db) : Controller
{
    private static readonly string[] SortOptions = ["due", "priority", "created", "title"];

    public async Task<IActionResult> Index(
        string? q,
        TaskState? status,
        TaskPriority? priority,
        string? category,
        string sort = "due")
    {
        var search = q?.Trim();
        if (search?.Length > 100)
        {
            search = search[..100];
        }

        if (!SortOptions.Contains(sort))
        {
            sort = "due";
        }

        IQueryable<TaskItem> query = db.Tasks.AsNoTracking();

        if (status is not null)
        {
            query = query.Where(t => t.Status == status);
        }

        if (priority is not null)
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(t =>
                EF.Functions.Like(t.Title, pattern) ||
                (t.Notes != null && EF.Functions.Like(t.Notes, pattern)) ||
                (t.Category != null && EF.Functions.Like(t.Category, pattern)));
        }

        query = sort switch
        {
            "priority" => query
                .OrderBy(t => t.Status == TaskState.Done)
                .ThenByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate == null)
                .ThenBy(t => t.DueDate)
                .ThenByDescending(t => t.CreatedAt),
            "created" => query
                .OrderBy(t => t.Status == TaskState.Done)
                .ThenByDescending(t => t.CreatedAt),
            "title" => query
                .OrderBy(t => t.Status == TaskState.Done)
                .ThenBy(t => t.Title),
            _ => query
                .OrderBy(t => t.Status == TaskState.Done)
                .ThenBy(t => t.DueDate == null)
                .ThenBy(t => t.DueDate)
                .ThenByDescending(t => t.Priority)
        };

        var today = DateOnly.FromDateTime(DateTime.Today);
        var categories = await GetCategoriesAsync();

        var model = new TaskListViewModel
        {
            Tasks = await query.ToListAsync(),
            Categories = categories,
            Search = search,
            Status = status,
            Priority = priority,
            Category = category,
            Sort = sort,
            Summary = new TaskSummary(
                Total: await db.Tasks.CountAsync(),
                Todo: await db.Tasks.CountAsync(t => t.Status == TaskState.Todo),
                InProgress: await db.Tasks.CountAsync(t => t.Status == TaskState.InProgress),
                Done: await db.Tasks.CountAsync(t => t.Status == TaskState.Done),
                DueToday: await db.Tasks.CountAsync(t => t.Status != TaskState.Done && t.DueDate == today),
                Overdue: await db.Tasks.CountAsync(t =>
                    t.Status != TaskState.Done && t.DueDate != null && t.DueDate < today))
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return task is null ? NotFound() : View(task);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new TaskItem());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Notes,Category,Priority,Status,DueDate")] TaskItem input)
    {
        input.Title = input.Title?.Trim() ?? string.Empty;
        input.Notes = input.Notes?.Trim();
        input.Category = input.Category?.Trim();

        if (input.Title.Length is > 0 and < 3)
        {
            ModelState.AddModelError(nameof(input.Title), "The title must be between 3 and 120 characters.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(input);
        }

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Title = input.Title,
            Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes,
            Category = string.IsNullOrWhiteSpace(input.Category) ? null : input.Category,
            Priority = input.Priority,
            Status = input.Status,
            DueDate = input.DueDate,
            CreatedAt = now,
            UpdatedAt = now,
            CompletedAt = input.Status == TaskState.Done ? now : null
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        TempData["Message"] = $"“{task.Title}” was added to the ledger.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync();
        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Notes,Category,Priority,Status,DueDate")] TaskItem input)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
        {
            return NotFound();
        }

        input.Title = input.Title?.Trim() ?? string.Empty;
        input.Notes = input.Notes?.Trim();
        input.Category = input.Category?.Trim();

        if (input.Title.Length is > 0 and < 3)
        {
            ModelState.AddModelError(nameof(input.Title), "The title must be between 3 and 120 characters.");
        }

        if (!ModelState.IsValid)
        {
            input.Id = id;
            await PopulateCategoriesAsync();
            return View(input);
        }

        var now = DateTime.UtcNow;
        var wasDone = task.Status == TaskState.Done;

        task.Title = input.Title;
        task.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes;
        task.Category = string.IsNullOrWhiteSpace(input.Category) ? null : input.Category;
        task.Priority = input.Priority;
        task.Status = input.Status;
        task.DueDate = input.DueDate;
        task.UpdatedAt = now;

        if (input.Status == TaskState.Done)
        {
            task.CompletedAt ??= now;
        }
        else if (wasDone)
        {
            task.CompletedAt = null;
        }

        await db.SaveChangesAsync();

        TempData["Message"] = $"“{task.Title}” was updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, string? returnUrl)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        if (task.Status == TaskState.Done)
        {
            task.Status = TaskState.Todo;
            task.CompletedAt = null;
            TempData["Message"] = $"“{task.Title}” is back on the list.";
        }
        else
        {
            task.Status = TaskState.Done;
            task.CompletedAt = now;
            TempData["Message"] = $"“{task.Title}” is done. Nicely closed out.";
        }

        task.UpdatedAt = now;
        await db.SaveChangesAsync();

        return RedirectBack(returnUrl);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return task is null ? NotFound() : View(task);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
        {
            return NotFound();
        }

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();

        TempData["Message"] = $"“{task.Title}” was removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCompleted()
    {
        var completed = await db.Tasks.Where(t => t.Status == TaskState.Done).ToListAsync();
        if (completed.Count > 0)
        {
            db.Tasks.RemoveRange(completed);
            await db.SaveChangesAsync();
            TempData["Message"] = completed.Count == 1
                ? "Cleared the completed task."
                : $"Cleared {completed.Count} completed tasks.";
        }
        else
        {
            TempData["Message"] = "There were no completed tasks to clear.";
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectBack(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetCategoriesAsync() =>
        await db.Tasks
            .AsNoTracking()
            .Where(t => t.Category != null && t.Category != "")
            .Select(t => t.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

    private async Task PopulateCategoriesAsync() =>
        ViewBag.Categories = await GetCategoriesAsync();
}
