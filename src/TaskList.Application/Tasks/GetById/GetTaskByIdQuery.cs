using TaskList.Application.Abstractions.Messaging;

namespace TaskList.Application.Tasks.GetById;

/// <summary>UC-07: a task with its category summary and <c>IsOverdue</c>.</summary>
public sealed record GetTaskByIdQuery(Guid Id) : IQuery<TaskResponse>;
