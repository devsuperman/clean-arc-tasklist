using TaskList.Domain.Common;
using TaskList.Domain.Tasks;

namespace TaskList.Domain.UnitTests.Tasks;

public sealed class TaskItemStatusTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 10, 23);

    public static TheoryData<TaskStatus, TaskStatus, bool> AllTransitions => new()
    {
        { TaskStatus.Pending, TaskStatus.Pending, false },
        { TaskStatus.Pending, TaskStatus.InProgress, true },
        { TaskStatus.Pending, TaskStatus.Done, true },
        { TaskStatus.InProgress, TaskStatus.Pending, true },
        { TaskStatus.InProgress, TaskStatus.InProgress, false },
        { TaskStatus.InProgress, TaskStatus.Done, true },
        { TaskStatus.Done, TaskStatus.Pending, true },
        { TaskStatus.Done, TaskStatus.InProgress, false },
        { TaskStatus.Done, TaskStatus.Done, false },
    };

    [Theory] // BR-T05: every pair of statuses
    [MemberData(nameof(AllTransitions))]
    public void ChangeStatus_FollowsTransitionTable(TaskStatus from, TaskStatus to, bool allowed)
    {
        var task = NewTaskWithStatus(from);
        var later = Now.AddHours(2);

        var result = to switch
        {
            TaskStatus.Pending => task.Reopen(later),
            TaskStatus.InProgress => task.Start(later),
            _ => task.Complete(later),
        };

        TaskItem.CanTransition(from, to).ShouldBe(allowed);
        if (allowed)
        {
            result.IsSuccess.ShouldBeTrue();
            task.Status.ShouldBe(to);
            task.UpdatedAt.ShouldBe(later);
        }
        else
        {
            result.Error.ShouldBe(TaskErrors.InvalidStatusTransition(from, to));
            result.Error!.Type.ShouldBe(ErrorType.Conflict);
            task.Status.ShouldBe(from);
        }
    }

    [Fact] // BR-T07
    public void Complete_SetsCompletedAt()
    {
        var task = NewTaskWithStatus(TaskStatus.InProgress);
        var completedAt = Now.AddHours(3);

        task.Complete(completedAt);

        task.CompletedAt.ShouldBe(completedAt);
    }

    [Fact] // BR-T07
    public void Reopen_FromDone_ClearsCompletedAt()
    {
        var task = NewTaskWithStatus(TaskStatus.Done);

        task.Reopen(Now.AddHours(3));

        task.Status.ShouldBe(TaskStatus.Pending);
        task.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void FailedTransition_KeepsCompletedAt()
    {
        var task = NewTaskWithStatus(TaskStatus.Done);
        var completedAt = task.CompletedAt;

        task.Start(Now.AddHours(3));

        task.CompletedAt.ShouldBe(completedAt);
    }

    [Theory]
    [InlineData(TaskStatus.Pending, -1, true)]
    [InlineData(TaskStatus.InProgress, -1, true)]
    [InlineData(TaskStatus.Done, -1, false)]
    [InlineData(TaskStatus.Pending, 0, false)]
    [InlineData(TaskStatus.Pending, 1, false)]
    public void IsOverdue_OnlyWhenNotDoneAndDueBeforeToday(TaskStatus status, int dueInDays, bool expected)
    {
        var task = NewTaskWithStatus(status, dueDate: Today);

        task.IsOverdue(Today.AddDays(-dueInDays)).ShouldBe(expected);
    }

    [Fact]
    public void IsOverdue_WithoutDueDate_IsFalse() =>
        NewTaskWithStatus(TaskStatus.Pending).IsOverdue(Today.AddYears(1)).ShouldBeFalse();

    private static TaskItem NewTaskWithStatus(TaskStatus status, DateOnly? dueDate = null)
    {
        var task = TaskItem.Create("Read book", null, Guid.NewGuid(), null, dueDate, Now).Value;
        var result = status switch
        {
            TaskStatus.InProgress => task.Start(Now.AddHours(1)),
            TaskStatus.Done => task.Complete(Now.AddHours(1)),
            _ => Result.Success(),
        };
        result.IsSuccess.ShouldBeTrue();
        return task;
    }
}
