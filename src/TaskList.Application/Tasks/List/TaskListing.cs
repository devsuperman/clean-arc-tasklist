using TaskList.Application.Abstractions.Data;
using TaskList.Application.Common.Paging;

namespace TaskList.Application.Tasks.List;

/// <summary>The task listing shared by UC-08 and UC-14: filter and sort on tasks, then project and page.</summary>
internal static class TaskListing
{
    public static Task<PagedResult<TaskResponse>> ListTasksAsync(
        this IReadDbContext db,
        Guid? categoryId,
        ITaskListQuery query,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var tasks = db.Tasks
            .WhereCategory(categoryId)
            .ApplyListQuery(query, today)
            .SelectResponse(db.Categories, today);

        return db.ToPagedResultAsync(tasks, query, cancellationToken);
    }
}
