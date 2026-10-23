using TaskList.Domain.Common;

namespace TaskList.Domain.Tasks;

public static class TaskErrors
{
    public static Error NotFound(Guid id) =>
        new("Task.NotFound", $"Task '{id}' was not found.", ErrorType.NotFound);

    public static Error InvalidStatusTransition(TaskStatus from, TaskStatus to) =>
        new(
            "Task.InvalidStatusTransition",
            $"A task cannot change status from '{from}' to '{to}'.",
            ErrorType.Conflict);

    public static readonly Error CompletedTaskIsReadOnly = new(
        "Task.CompletedTaskIsReadOnly",
        "A completed task cannot be edited. Reopen it first.",
        ErrorType.Conflict);

    public static readonly Error DueDateInPast = new(
        "Task.DueDateInPast",
        "The due date cannot be in the past.",
        ErrorType.Validation);

    public static readonly ValidationError TitleInvalidLength = ValidationError.ForField(
        "title",
        $"Title must have between {TaskItem.TitleMinLength} and {TaskItem.TitleMaxLength} characters.");

    public static readonly ValidationError DescriptionTooLong = ValidationError.ForField(
        "description",
        $"Description must have at most {TaskItem.DescriptionMaxLength} characters.");
}
