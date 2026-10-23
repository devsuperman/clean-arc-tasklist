using TaskList.Application.Abstractions.Messaging;

namespace TaskList.Application.Categories.Update;

/// <summary>UC-04: replaces the name, description and color of a category.</summary>
public sealed record UpdateCategoryCommand(Guid Id, string? Name, string? Description, string? Color) : ICommand;
