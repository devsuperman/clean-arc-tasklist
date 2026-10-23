using TaskList.Domain.Categories;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Categories;

public static class CategoryProjections
{
    /// <summary>
    /// Projects categories to <see cref="CategoryResponse"/>, counting all tasks and the ones not <c>Done</c> (BR-C07).
    /// Translated to a single SQL query.
    /// </summary>
    public static IQueryable<CategoryResponse> SelectResponse(
        this IQueryable<Category> categories,
        IQueryable<TaskItem> tasks) =>
        categories.Select(category => new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.Color,
            tasks.Count(task => task.CategoryId == category.Id),
            tasks.Count(task => task.CategoryId == category.Id && task.Status != TaskStatus.Done),
            category.CreatedAt,
            category.UpdatedAt));
}
