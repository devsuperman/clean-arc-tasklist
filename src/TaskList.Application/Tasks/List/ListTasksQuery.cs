using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common.Paging;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.List;

/// <summary>UC-08: list tasks with filters, sorting and paging (BR-L01–BR-L05). All filters are optional.</summary>
public sealed record ListTasksQuery(
    Guid? CategoryId = null,
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
