using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common.Paging;
using TaskList.Application.Tasks.List;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.ListByCategory;

/// <summary>UC-14: the same listing as UC-08 with <see cref="CategoryId"/> fixed.</summary>
public sealed record ListCategoryTasksQuery(
    Guid CategoryId,
    TaskStatus? Status = null,
    TaskPriority? Priority = null,
    DateOnly? DueFrom = null,
    DateOnly? DueTo = null,
    bool? Overdue = null,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null,
    int Page = Paging.DefaultPage,
    int PageSize = Paging.DefaultPageSize) : IQuery<PagedResult<TaskResponse>>, ITaskListQuery;
