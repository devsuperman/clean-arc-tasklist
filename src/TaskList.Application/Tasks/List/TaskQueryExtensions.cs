using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.List;

/// <summary>
/// Composable filters and sorting on <see cref="TaskItem"/> (BR-L03, BR-L05). Every filter is skipped when its value
/// is <c>null</c>. Apply them <b>before</b> projecting with <see cref="TaskProjections.SelectResponse"/>.
/// </summary>
public static class TaskQueryExtensions
{
    /// <summary>Applies every filter and the sort of <paramref name="query"/>; overdue is calculated for <paramref name="today"/>.</summary>
    public static IQueryable<TaskItem> ApplyListQuery(this IQueryable<TaskItem> tasks, ITaskListQuery query, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(query);
        return tasks
            .WhereStatus(query.Status)
            .WherePriority(query.Priority)
            .WhereDueFrom(query.DueFrom)
            .WhereDueTo(query.DueTo)
            .WhereOverdue(query.Overdue, today)
            .WhereSearch(query.Search)
            .ApplySort(query.SortBy, query.SortDirection);
    }

    public static IQueryable<TaskItem> WhereCategory(this IQueryable<TaskItem> tasks, Guid? categoryId) =>
        categoryId is null ? tasks : tasks.Where(task => task.CategoryId == categoryId);

    public static IQueryable<TaskItem> WhereStatus(this IQueryable<TaskItem> tasks, TaskStatus? status) =>
        status is null ? tasks : tasks.Where(task => task.Status == status);

    public static IQueryable<TaskItem> WherePriority(this IQueryable<TaskItem> tasks, TaskPriority? priority) =>
        priority is null ? tasks : tasks.Where(task => task.Priority == priority);

    /// <summary>Tasks due on or after <paramref name="dueFrom"/>; tasks without due date are excluded.</summary>
    public static IQueryable<TaskItem> WhereDueFrom(this IQueryable<TaskItem> tasks, DateOnly? dueFrom) =>
        dueFrom is null ? tasks : tasks.Where(task => task.DueDate != null && task.DueDate >= dueFrom);

    /// <summary>Tasks due on or before <paramref name="dueTo"/>; tasks without due date are excluded.</summary>
    public static IQueryable<TaskItem> WhereDueTo(this IQueryable<TaskItem> tasks, DateOnly? dueTo) =>
        dueTo is null ? tasks : tasks.Where(task => task.DueDate != null && task.DueDate <= dueTo);

    /// <summary>When <paramref name="overdue"/> is <c>true</c>, keeps tasks not done and due before <paramref name="today"/>.</summary>
    public static IQueryable<TaskItem> WhereOverdue(this IQueryable<TaskItem> tasks, bool? overdue, DateOnly today) =>
        overdue is true
            ? tasks.Where(task => task.Status != TaskStatus.Done && task.DueDate != null && task.DueDate < today)
            : tasks;

    /// <summary>Case-insensitive "contains" on title and description (BR-L03). Blank search is ignored.</summary>
    public static IQueryable<TaskItem> WhereSearch(this IQueryable<TaskItem> tasks, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return tasks;
        }

        var term = search.Trim().ToLowerInvariant();

#pragma warning disable CA1304, CA1311, CA1862 // ToLower() is translated to SQL lower(); StringComparison is not translatable.
        return tasks.Where(task =>
            task.Title.ToLower().Contains(term)
            || (task.Description != null && task.Description.ToLower().Contains(term)));
#pragma warning restore CA1304, CA1311, CA1862
    }

    /// <summary>
    /// Sorts by <paramref name="sortBy"/> (default <c>createdAt</c>) in <paramref name="sortDirection"/> (default
    /// <c>desc</c>), with the id as a stable tiebreaker (BR-L05). Tasks without due date go last in both directions
    /// when sorting by due date. Priority is sorted by rank (Low &lt; Medium &lt; High), not by its stored name.
    /// </summary>
    public static IQueryable<TaskItem> ApplySort(this IQueryable<TaskItem> tasks, string? sortBy, string? sortDirection)
    {
        var descending = sortDirection is null || TaskSort.IsField(sortDirection, TaskSort.Descending);

        IOrderedQueryable<TaskItem> sorted;
        if (TaskSort.IsField(sortBy, TaskSort.DueDate))
        {
            var withDueDateFirst = tasks.OrderBy(task => task.DueDate == null);
            sorted = descending
                ? withDueDateFirst.ThenByDescending(task => task.DueDate)
                : withDueDateFirst.ThenBy(task => task.DueDate);
        }
        else if (TaskSort.IsField(sortBy, TaskSort.Priority))
        {
            // Priority is stored as a string, so sort by rank. EF Core translates this to a CASE expression.
            sorted = descending
                ? tasks.OrderByDescending(task => task.Priority == TaskPriority.High ? 3 : task.Priority == TaskPriority.Medium ? 2 : 1)
                : tasks.OrderBy(task => task.Priority == TaskPriority.High ? 3 : task.Priority == TaskPriority.Medium ? 2 : 1);
        }
        else if (TaskSort.IsField(sortBy, TaskSort.Title))
        {
            sorted = descending ? tasks.OrderByDescending(task => task.Title) : tasks.OrderBy(task => task.Title);
        }
        else
        {
            sorted = descending ? tasks.OrderByDescending(task => task.CreatedAt) : tasks.OrderBy(task => task.CreatedAt);
        }

        return sorted.ThenBy(task => task.Id);
    }
}
