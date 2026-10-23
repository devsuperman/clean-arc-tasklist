using TaskList.Domain.Tasks;

namespace TaskList.Domain.UnitTests.Tasks;

public sealed class TaskItemEditTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Now = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 10, 23);

    [Fact]
    public void UpdateDetails_WithValidData_ReplacesDetails()
    {
        var task = TaskItem.Create("Old", "Old desc", Guid.NewGuid(), TaskPriority.Low, null, CreatedAt).Value;

        var result = task.UpdateDetails("  New title ", " New desc ", TaskPriority.High, Today.AddDays(2), Now);

        result.IsSuccess.ShouldBeTrue();
        task.Title.ShouldBe("New title");
        task.Description.ShouldBe("New desc");
        task.Priority.ShouldBe(TaskPriority.High);
        task.DueDate.ShouldBe(Today.AddDays(2));
        task.UpdatedAt.ShouldBe(Now);
    }

    [Fact]
    public void UpdateDetails_WithoutPriority_FallsBackToMedium()
    {
        var task = TaskItem.Create("Title", null, Guid.NewGuid(), TaskPriority.High, null, CreatedAt).Value;

        task.UpdateDetails("Title", null, null, null, Now);

        task.Priority.ShouldBe(TaskPriority.Medium);
    }

    [Fact] // BR-T04 (update case)
    public void UpdateDetails_WithNewPastDueDate_FailsWithDueDateInPast()
    {
        var task = TaskItem.Create("Title", null, Guid.NewGuid(), null, Today, CreatedAt).Value;

        task.UpdateDetails("Title", null, null, Today.AddDays(-1), Now).Error.ShouldBe(TaskErrors.DueDateInPast);
        task.DueDate.ShouldBe(Today);
    }

    [Fact] // BR-T04 (update case): old tasks keep their past due date and can still be edited
    public void UpdateDetails_KeepingExistingPastDueDate_Succeeds()
    {
        var pastDueDate = new DateOnly(2026, 10, 5);
        var task = TaskItem.Create("Title", null, Guid.NewGuid(), null, pastDueDate, CreatedAt).Value;

        var result = task.UpdateDetails("New title", null, null, pastDueDate, Now);

        result.IsSuccess.ShouldBeTrue();
        task.Title.ShouldBe("New title");
    }

    [Fact] // BR-T04: clearing the due date is always allowed
    public void UpdateDetails_RemovingDueDate_Succeeds()
    {
        var task = TaskItem.Create("Title", null, Guid.NewGuid(), null, Today, CreatedAt).Value;

        task.UpdateDetails("Title", null, null, null, Now).IsSuccess.ShouldBeTrue();
        task.DueDate.ShouldBeNull();
    }

    [Fact]
    public void UpdateDetails_WithInvalidTitle_KeepsPreviousState()
    {
        var task = TaskItem.Create("Title", null, Guid.NewGuid(), TaskPriority.Low, null, CreatedAt).Value;

        task.UpdateDetails("x", null, TaskPriority.High, Today, Now).Error.ShouldBe(TaskErrors.TitleInvalidLength);

        task.Title.ShouldBe("Title");
        task.Priority.ShouldBe(TaskPriority.Low);
        task.DueDate.ShouldBeNull();
        task.UpdatedAt.ShouldBeNull();
    }

    [Fact] // BR-T08
    public void UpdateDetails_OnDoneTask_FailsWithCompletedTaskIsReadOnly()
    {
        var task = DoneTask();

        task.UpdateDetails("New title", null, null, null, Now).Error.ShouldBe(TaskErrors.CompletedTaskIsReadOnly);
        task.Title.ShouldBe("Title");
    }

    [Fact] // BR-T10
    public void MoveTo_ChangesCategory()
    {
        var task = TaskItem.Create("Title", null, Guid.NewGuid(), null, null, CreatedAt).Value;
        var newCategoryId = Guid.NewGuid();

        task.MoveTo(newCategoryId, Now).IsSuccess.ShouldBeTrue();

        task.CategoryId.ShouldBe(newCategoryId);
        task.UpdatedAt.ShouldBe(Now);
    }

    [Fact]
    public void MoveTo_SameCategory_IsNoOp()
    {
        var categoryId = Guid.NewGuid();
        var task = TaskItem.Create("Title", null, categoryId, null, null, CreatedAt).Value;

        task.MoveTo(categoryId, Now).IsSuccess.ShouldBeTrue();

        task.UpdatedAt.ShouldBeNull();
    }

    [Fact] // BR-T08, BR-T10
    public void MoveTo_OnDoneTask_FailsWithCompletedTaskIsReadOnly()
    {
        var task = DoneTask();
        var categoryId = task.CategoryId;

        task.MoveTo(Guid.NewGuid(), Now).Error.ShouldBe(TaskErrors.CompletedTaskIsReadOnly);
        task.CategoryId.ShouldBe(categoryId);
    }

    [Fact] // BR-T08: reopening makes the task editable again
    public void UpdateDetails_AfterReopen_Succeeds()
    {
        var task = DoneTask();
        task.Reopen(Now);

        task.UpdateDetails("New title", null, null, null, Now).IsSuccess.ShouldBeTrue();
    }

    private static TaskItem DoneTask()
    {
        var task = TaskItem.Create("Title", null, Guid.NewGuid(), null, null, CreatedAt).Value;
        task.Complete(CreatedAt.AddDays(1));
        return task;
    }
}
