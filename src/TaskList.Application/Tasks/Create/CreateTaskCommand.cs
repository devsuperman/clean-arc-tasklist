using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.Create;

/// <summary>UC-06. The status is not part of the command: a new task is always <c>Pending</c> (BR-T02).</summary>
public sealed record CreateTaskCommand(
    string? Title,
    string? Description,
    Guid CategoryId,
    TaskPriority? Priority,
    DateOnly? DueDate) : ICommand<Guid>;
