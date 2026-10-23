using TaskList.Application.Abstractions.Messaging;

namespace TaskList.Application.Categories.Create;

/// <summary>UC-01: creates a category and returns its id.</summary>
public sealed record CreateCategoryCommand(string? Name, string? Description, string? Color) : ICommand<Guid>;
