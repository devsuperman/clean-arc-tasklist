using TaskList.Application.Abstractions.Messaging;

namespace TaskList.Application.Tasks.ChangeStatus;

/// <summary>Complete a task (→ Done, UC-11); sets CompletedAt (BR-T07).</summary>
public sealed record CompleteTaskCommand(Guid TaskId) : ICommand;
