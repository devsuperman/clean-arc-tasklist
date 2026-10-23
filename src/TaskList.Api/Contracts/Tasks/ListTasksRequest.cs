using TaskList.Domain.Tasks;

namespace TaskList.Api.Contracts.Tasks;

/// <summary>Query string of <c>GET /tasks</c> (BR-L01–BR-L05).</summary>
public sealed record ListTasksRequest(
    Guid? CategoryId,
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
