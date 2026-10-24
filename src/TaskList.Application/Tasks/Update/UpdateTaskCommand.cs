using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.Update;

/// <summary>
/// UC-09: full update of the editable details, including the category (BR-T10).
/// The status is not part of the command; it only changes through the status actions (BR-T06).
/// </summary>
public sealed record UpdateTaskCommand(
    Guid TaskId,
    string? Title,
    string? Description,
    Guid CategoryId,
    TaskPriority? Priority,
    DateOnly? DueDate) : ICommand;
