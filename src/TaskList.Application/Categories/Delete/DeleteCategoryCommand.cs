using TaskList.Application.Abstractions.Messaging;

namespace TaskList.Application.Categories.Delete;

/// <summary>UC-05: permanently deletes a category that has no tasks (BR-C04, BR-C05).</summary>
public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
