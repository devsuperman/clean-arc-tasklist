using TaskList.Domain.Tasks;

namespace TaskList.Api.Contracts.Tasks;

/// <summary>Query string of <c>GET /categories/{id}/tasks</c>: the filters of <see cref="ListTasksRequest"/> without <c>categoryId</c>.</summary>
public sealed record ListCategoryTasksRequest(
    TaskStatus? Status,
    TaskPriority? Priority,
    DateOnly? DueFrom,
    DateOnly? DueTo,
    bool? Overdue,
    string? Search,
    string? SortBy,
    string? SortDirection,
    int Page = 1,
    int PageSize = 20);
