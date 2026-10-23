using TaskList.Domain.Tasks;

namespace TaskList.Api.Contracts.Tasks;

/// <summary>Body of <c>POST /api/v1/tasks</c>. A <c>status</c> sent by the client is ignored (BR-T02).</summary>
public sealed record CreateTaskRequest(
    string? Title,
    string? Description,
    Guid CategoryId,
    TaskPriority? Priority,
    DateOnly? DueDate);
