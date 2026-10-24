namespace TaskList.Application.Tasks.List;

/// <summary>Accepted values of <c>sortBy</c> and <c>sortDirection</c> (BR-L05).</summary>
public static class TaskSort
{
    public const string CreatedAt = "createdAt";
    public const string DueDate = "dueDate";
    public const string Priority = "priority";
    public const string Title = "title";

    public const string Ascending = "asc";
    public const string Descending = "desc";

    public static readonly IReadOnlyList<string> Fields = [CreatedAt, DueDate, Priority, Title];

    public static readonly IReadOnlyList<string> Directions = [Ascending, Descending];

    internal static bool IsField(string? value, string field) =>
        string.Equals(value, field, StringComparison.OrdinalIgnoreCase);
}
