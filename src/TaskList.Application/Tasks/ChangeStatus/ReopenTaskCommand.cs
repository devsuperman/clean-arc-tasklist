using TaskList.Application.Abstractions.Messaging;

namespace TaskList.Application.Tasks.ChangeStatus;

/// <summary>Reopen a task (→ Pending, UC-12); clears CompletedAt (BR-T07).</summary>
public sealed record ReopenTaskCommand(Guid TaskId) : ICommand;
