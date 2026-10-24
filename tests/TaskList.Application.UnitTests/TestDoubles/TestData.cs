using TaskList.Domain.Categories;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.TestDoubles;

/// <summary>Builders for valid domain objects.</summary>
public static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);

    public static readonly DateOnly Today = new(2026, 10, 23);

    public static Category Category(string name = "Work", string? color = null, DateTimeOffset? createdAt = null) =>
        Domain.Categories.Category.Create(name, null, color, createdAt ?? Now).Value;

    public static TaskItem Task(
        Guid categoryId,
        string title = "Read book",
        TaskStatus status = TaskStatus.Pending,
        TaskPriority priority = TaskPriority.Medium,
        DateOnly? dueDate = null,
        DateTimeOffset? createdAt = null)
    {
        var created = createdAt ?? Now;
        var task = TaskItem.Create(title, null, categoryId, priority, dueDate, created).Value;
        _ = status switch
        {
            TaskStatus.InProgress => task.Start(created),
            TaskStatus.Done => task.Complete(created),
            _ => Domain.Common.Result.Success(),
        };
        return task;
    }
}
