using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks;

/// <summary>A task as returned by the API.</summary>
public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    CategorySummary Category,
    TaskStatus Status,
    TaskPriority Priority,
    DateOnly? DueDate,
    bool IsOverdue,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>The category a task belongs to, embedded in <see cref="TaskResponse"/>.</summary>
public sealed record CategorySummary(Guid Id, string Name, string? Color);
