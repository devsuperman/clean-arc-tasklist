using TaskList.Domain.Common;

namespace TaskList.Domain.Tasks;

/// <summary>A task. Named <c>TaskItem</c> to avoid clashing with <see cref="System.Threading.Tasks.Task"/>.</summary>
public sealed class TaskItem : Entity
{
    public const int TitleMinLength = 3;
    public const int TitleMaxLength = 120;
    public const int DescriptionMaxLength = 1000;

    // BR-T05: every allowed (from, to) status change. Anything else, including "to the same status", is rejected.
    private static readonly HashSet<(TaskStatus From, TaskStatus To)> AllowedTransitions =
    [
        (TaskStatus.Pending, TaskStatus.InProgress),
        (TaskStatus.Pending, TaskStatus.Done),
        (TaskStatus.InProgress, TaskStatus.Pending),
        (TaskStatus.InProgress, TaskStatus.Done),
        (TaskStatus.Done, TaskStatus.Pending),
    ];

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

    /// <summary>
    /// Replaces the editable details (full update; a missing priority falls back to <c>Medium</c> as in BR-T03).
    /// Done tasks are read-only (BR-T08). A past due date is only accepted if it is unchanged (BR-T04).
    /// </summary>
    public Result UpdateDetails(
        string? title,
        string? description,
        TaskPriority? priority,
        DateOnly? dueDate,
        DateTimeOffset now)
    {
        if (Status == TaskStatus.Done)
        {
            return TaskErrors.CompletedTaskIsReadOnly;
        }

        if (dueDate < Today(now) && dueDate != DueDate)
        {
            return TaskErrors.DueDateInPast;
        }

        var result = ApplyText(title, description);
        if (result.IsFailure)
        {
            return result;
        }

        Priority = priority ?? TaskPriority.Medium;
        DueDate = dueDate;
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>Moves the task to another category (BR-T10). Done tasks are read-only (BR-T08).</summary>
    public Result MoveTo(Guid categoryId, DateTimeOffset now)
    {
        if (Status == TaskStatus.Done)
        {
            return TaskErrors.CompletedTaskIsReadOnly;
        }

        if (categoryId != CategoryId)
        {
            CategoryId = categoryId;
            UpdatedAt = now;
        }

        return Result.Success();
    }

    public static bool CanTransition(TaskStatus from, TaskStatus to) => AllowedTransitions.Contains((from, to));

    /// <summary>Start working on the task (<c>→ InProgress</c>).</summary>
    public Result Start(DateTimeOffset now) => TransitionTo(TaskStatus.InProgress, now);

    /// <summary>Complete the task (<c>→ Done</c>) and record <see cref="CompletedAt"/> (BR-T07).</summary>
    public Result Complete(DateTimeOffset now) => TransitionTo(TaskStatus.Done, now);

    /// <summary>Go back to <c>Pending</c> from <c>InProgress</c> or <c>Done</c>; clears <see cref="CompletedAt"/> (BR-T07).</summary>
    public Result Reopen(DateTimeOffset now) => TransitionTo(TaskStatus.Pending, now);

    /// <summary>Not <c>Done</c> and due before <paramref name="today"/>. Calculated, never stored.</summary>
    public bool IsOverdue(DateOnly today) => Status != TaskStatus.Done && DueDate < today;

    private static DateOnly Today(DateTimeOffset now) => DateOnly.FromDateTime(now.UtcDateTime);

    private Result TransitionTo(TaskStatus target, DateTimeOffset now)
    {
        if (!CanTransition(Status, target))
        {
            return TaskErrors.InvalidStatusTransition(Status, target);
        }

        Status = target;
        CompletedAt = target == TaskStatus.Done ? now : null;
        UpdatedAt = now;
        return Result.Success();
    }

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
