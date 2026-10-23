using TaskList.Domain.Common;

namespace TaskList.Domain.Tasks;

/// <summary>A task. Named <c>TaskItem</c> to avoid clashing with <see cref="System.Threading.Tasks.Task"/>.</summary>
public sealed class TaskItem : Entity
{
    public const int TitleMinLength = 3;
    public const int TitleMaxLength = 120;
    public const int DescriptionMaxLength = 1000;

    private TaskItem(Guid id, Guid categoryId, DateTimeOffset createdAt)
        : base(id)
    {
        CategoryId = categoryId;
        CreatedAt = createdAt;
    }

    // Required by EF Core.
    private TaskItem()
    {
    }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid CategoryId { get; private set; }

    public TaskStatus Status { get; private set; }

    public TaskPriority Priority { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Result<TaskItem> Create(
        string? title,
        string? description,
        Guid categoryId,
        TaskPriority? priority,
        DateOnly? dueDate,
        DateTimeOffset now)
    {
        // BR-T04: on creation the due date cannot be in the past (today is allowed).
        if (dueDate < Today(now))
        {
            return TaskErrors.DueDateInPast;
        }

        var task = new TaskItem(Guid.CreateVersion7(now), categoryId, now)
        {
            Status = TaskStatus.Pending,                    // BR-T02
            Priority = priority ?? TaskPriority.Medium,     // BR-T03
            DueDate = dueDate,
        };

        var result = task.ApplyText(title, description);
        return result.IsSuccess ? task : result.Error;
    }

    private static DateOnly Today(DateTimeOffset now) => DateOnly.FromDateTime(now.UtcDateTime);

    private Result ApplyText(string? title, string? description)
    {
        var normalizedTitle = Text.Normalize(title);
        if (normalizedTitle.Length is < TitleMinLength or > TitleMaxLength)
        {
            return TaskErrors.TitleInvalidLength;
        }

        var normalizedDescription = Text.NormalizeOptional(description);
        if (normalizedDescription?.Length > DescriptionMaxLength)
        {
            return TaskErrors.DescriptionTooLong;
        }

        Title = normalizedTitle;
        Description = normalizedDescription;
        return Result.Success();
    }
}
