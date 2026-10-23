using TaskList.Domain.Categories;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks;

public static class TaskProjections
{
    /// <summary>
    /// Projects tasks to <see cref="TaskResponse"/> with their category summary and <c>IsOverdue</c> calculated for
    /// <paramref name="today"/>. Apply filters, sorting and paging on <see cref="TaskItem"/> <b>before</b> calling this:
    /// the projection keeps the order of the source query.
    /// </summary>
    public static IQueryable<TaskResponse> SelectResponse(
        this IQueryable<TaskItem> tasks,
        IQueryable<Category> categories,
        DateOnly today) =>
        tasks.Select(task => new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            categories
                .Where(category => category.Id == task.CategoryId)
                .Select(category => new CategorySummary(category.Id, category.Name, category.Color))
                .First(),
            task.Status,
            task.Priority,
            task.DueDate,
            task.Status != TaskStatus.Done && task.DueDate != null && task.DueDate < today,
            task.CompletedAt,
            task.CreatedAt,
            task.UpdatedAt));
}
