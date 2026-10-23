using TaskList.Domain.Tasks;

namespace TaskList.Api.Contracts.Tasks;

/// <summary>Body of <c>PUT /api/v1/tasks/{id}</c>. A <c>status</c> sent by the client is ignored (BR-T06).</summary>
public sealed record UpdateTaskRequest(
    string? Title,
    string? Description,
    Guid CategoryId,
    TaskPriority? Priority,
    DateOnly? DueDate);
