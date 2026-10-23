using TaskList.Application.Abstractions.Messaging;

namespace TaskList.Application.Tasks.ChangeStatus;

/// <summary>Start working on a task (→ InProgress, UC-10).</summary>
public sealed record StartTaskCommand(Guid TaskId) : ICommand;
