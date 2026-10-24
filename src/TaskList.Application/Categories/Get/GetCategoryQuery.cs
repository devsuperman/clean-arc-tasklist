using TaskList.Application.Abstractions.Messaging;

namespace TaskList.Application.Categories.Get;

/// <summary>UC-02: reads one category with its task counters (BR-C07).</summary>
public sealed record GetCategoryQuery(Guid Id) : IQuery<CategoryResponse>;
