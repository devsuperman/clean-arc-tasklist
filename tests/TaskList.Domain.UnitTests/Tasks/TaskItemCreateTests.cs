using TaskList.Domain.Common;
using TaskList.Domain.Tasks;

namespace TaskList.Domain.UnitTests.Tasks;

public sealed class TaskItemCreateTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 10, 23);
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_SetsFields()
    {
        var task = TaskItem.Create("Read book", "Chapters 1-5", CategoryId, TaskPriority.High, Today.AddDays(3), Now).Value;

        task.Id.ShouldNotBe(Guid.Empty);
        task.Title.ShouldBe("Read book");
        task.Description.ShouldBe("Chapters 1-5");
        task.CategoryId.ShouldBe(CategoryId);
        task.Priority.ShouldBe(TaskPriority.High);
        task.DueDate.ShouldBe(Today.AddDays(3));
        task.CreatedAt.ShouldBe(Now);
        task.UpdatedAt.ShouldBeNull();
        task.CompletedAt.ShouldBeNull();
    }

    [Fact] // BR-T02
    public void Create_AlwaysStartsPending() =>
        TaskItem.Create("Read book", null, CategoryId, null, null, Now).Value.Status.ShouldBe(TaskStatus.Pending);

    [Fact] // BR-T03
    public void Create_WithoutPriority_DefaultsToMedium() =>
        TaskItem.Create("Read book", null, CategoryId, null, null, Now).Value.Priority.ShouldBe(TaskPriority.Medium);

    [Fact] // BR-T04
    public void Create_WithDueDateYesterday_FailsWithDueDateInPast()
    {
        var result = TaskItem.Create("Read book", null, CategoryId, null, Today.AddDays(-1), Now);

        result.Error.ShouldBe(TaskErrors.DueDateInPast);
        result.Error!.Type.ShouldBe(ErrorType.Validation);
    }

    [Fact] // BR-T04
    public void Create_WithDueDateToday_Succeeds() =>
        TaskItem.Create("Read book", null, CategoryId, null, Today, Now).IsSuccess.ShouldBeTrue();

    [Fact] // BR-T04, BR-G03: "today" is the UTC date, whatever the offset of "now"
    public void Create_UsesUtcDateForToday()
    {
        var lateEveningInBrazil = new DateTimeOffset(2026, 10, 22, 23, 0, 0, TimeSpan.FromHours(-3)); // 02:00 UTC on the 23rd

        TaskItem.Create("Read book", null, CategoryId, null, new DateOnly(2026, 10, 22), lateEveningInBrazil)
            .Error.ShouldBe(TaskErrors.DueDateInPast);
    }

    [Fact] // BR-G01
    public void Create_TrimsTitleAndDescription()
    {
        var task = TaskItem.Create("  Read book  ", "   ", CategoryId, null, null, Now).Value;

        task.Title.ShouldBe("Read book");
        task.Description.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData("ab")]
    [InlineData(" ab ")]
    public void Create_WithTooShortTitle_FailsValidation(string? title) =>
        TaskItem.Create(title, null, CategoryId, null, null, Now).Error.ShouldBe(TaskErrors.TitleInvalidLength);

    [Fact]
    public void Create_LengthBoundaries()
    {
        TaskItem.Create("abc", null, CategoryId, null, null, Now).IsSuccess.ShouldBeTrue();
        TaskItem.Create(new string('t', 120), null, CategoryId, null, null, Now).IsSuccess.ShouldBeTrue();
        TaskItem.Create(new string('t', 121), null, CategoryId, null, null, Now).Error.ShouldBe(TaskErrors.TitleInvalidLength);
        TaskItem.Create("abc", new string('d', 1000), CategoryId, null, null, Now).IsSuccess.ShouldBeTrue();
        TaskItem.Create("abc", new string('d', 1001), CategoryId, null, null, Now).Error.ShouldBe(TaskErrors.DescriptionTooLong);
    }

    [Fact]
    public void Errors_FollowTheErrorCatalog()
    {
        TaskErrors.NotFound(Guid.NewGuid()).Code.ShouldBe("Task.NotFound");
        TaskErrors.NotFound(Guid.NewGuid()).Type.ShouldBe(ErrorType.NotFound);
        TaskErrors.InvalidStatusTransition(TaskStatus.Done, TaskStatus.InProgress).Code.ShouldBe("Task.InvalidStatusTransition");
        TaskErrors.InvalidStatusTransition(TaskStatus.Done, TaskStatus.InProgress).Type.ShouldBe(ErrorType.Conflict);
        TaskErrors.CompletedTaskIsReadOnly.Code.ShouldBe("Task.CompletedTaskIsReadOnly");
        TaskErrors.CompletedTaskIsReadOnly.Type.ShouldBe(ErrorType.Conflict);
        TaskErrors.DueDateInPast.Code.ShouldBe("Task.DueDateInPast");
    }
}
