using TaskList.Application.Abstractions.Messaging;

namespace TaskList.Application.Tasks.Delete;

/// <summary>UC-13: hard-delete a task in any status (BR-T09).</summary>
public sealed record DeleteTaskCommand(Guid Id) : ICommand;
