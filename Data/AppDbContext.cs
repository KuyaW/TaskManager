using Microsoft.EntityFrameworkCore;
using MvcApp.Models;

namespace MvcApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.DueDate);
            entity.HasIndex(t => t.Category);
        });
    }
}
