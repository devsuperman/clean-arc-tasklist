using TaskList.Application.Common.Paging;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.List;

/// <summary>
/// Filters, sorting and paging shared by every task listing (BR-L01–BR-L05): <see cref="ListTasksQuery"/> and
/// the per-category listing. Validated by <see cref="TaskListValidation.ApplyTaskListRules{T}"/> and applied by
/// <see cref="TaskQueryExtensions.ApplyListQuery"/>.
/// </summary>
public interface ITaskListQuery : IPagedQuery
{
    TaskStatus? Status { get; }

    TaskPriority? Priority { get; }

    DateOnly? DueFrom { get; }

    DateOnly? DueTo { get; }

    /// <summary><c>true</c> keeps only overdue tasks; <c>null</c> or <c>false</c> does not filter.</summary>
    bool? Overdue { get; }

    string? Search { get; }

    /// <summary>One of <see cref="TaskSort.Fields"/> (case-insensitive). Default <c>createdAt</c>.</summary>
    string? SortBy { get; }

    /// <summary><c>asc</c> or <c>desc</c> (case-insensitive). Default <c>desc</c>.</summary>
    string? SortDirection { get; }
}
