namespace TaskList.Application.Categories;

/// <summary>A category as returned by the API, with its task counters (BR-C07).</summary>
public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Color,
    int TaskCount,
    int PendingTaskCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
