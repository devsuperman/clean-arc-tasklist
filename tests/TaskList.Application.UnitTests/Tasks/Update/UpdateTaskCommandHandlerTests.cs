using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using TaskList.Application.Abstractions.Data;
using TaskList.Application.Tasks.Update;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.Tasks.Update;

public sealed class UpdateTaskCommandHandlerTests
{
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now);
    private readonly Guid _categoryId = Guid.NewGuid();

    private UpdateTaskCommandHandler Handler => new(_tasks, _categories, _unitOfWork, _timeProvider);

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Handle_ValidCommand_UpdatesDetailsAndSaves()
    {
        var task = SeedTask();
        _timeProvider.Advance(TimeSpan.FromHours(1));

        var result = await Handler.Handle(
            new UpdateTaskCommand(task.Id, " New title ", "Notes", _categoryId, TaskPriority.High, TestData.Today.AddDays(3)),
            CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        task.Title.ShouldBe("New title");
        task.Description.ShouldBe("Notes");
        task.Priority.ShouldBe(TaskPriority.High);
        task.DueDate.ShouldBe(TestData.Today.AddDays(3));
        task.UpdatedAt.ShouldBe(_timeProvider.GetUtcNow());
        await _categories.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-T10
    public async Task Handle_NewExistingCategory_MovesTask()
    {
        var task = SeedTask();
        var other = Guid.NewGuid();
        _categories.ExistsAsync(other, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.Handle(new UpdateTaskCommand(task.Id, "Read book", null, other, null, null), CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        task.CategoryId.ShouldBe(other);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TaskDoesNotExist_ReturnsTaskNotFound()
    {
        var id = Guid.NewGuid();

        var result = await Handler.Handle(new UpdateTaskCommand(id, "Read book", null, _categoryId, null, null), CancellationToken);

        result.Error.ShouldBe(TaskErrors.NotFound(id));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-T01
    public async Task Handle_NewCategoryDoesNotExist_ReturnsCategoryNotFound()
    {
        var task = SeedTask();

        var result = await Handler.Handle(new UpdateTaskCommand(task.Id, "New title", null, Guid.NewGuid(), null, null), CancellationToken);

        result.Error.ShouldNotBeNull().Code.ShouldBe("Category.NotFound");
        task.Title.ShouldBe("Read book");
        task.CategoryId.ShouldBe(_categoryId);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-T08
    public async Task Handle_DoneTask_ReturnsCompletedTaskIsReadOnly()
    {
        var task = SeedTask(TaskStatus.Done);

        var result = await Handler.Handle(new UpdateTaskCommand(task.Id, "New title", null, _categoryId, null, null), CancellationToken);

        result.Error.ShouldBe(TaskErrors.CompletedTaskIsReadOnly);
        task.Title.ShouldBe("Read book");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-T04
    public async Task Handle_UnchangedPastDueDate_IsAccepted()
    {
        var task = SeedTask(dueDate: TestData.Today, createdAt: TestData.Now);
        _timeProvider.Advance(TimeSpan.FromDays(5));

        var result = await Handler.Handle(
            new UpdateTaskCommand(task.Id, "New title", null, _categoryId, null, TestData.Today),
            CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        task.DueDate.ShouldBe(TestData.Today);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-T04
    public async Task Handle_NewPastDueDate_ReturnsDueDateInPast()
    {
        var task = SeedTask();

        var result = await Handler.Handle(
            new UpdateTaskCommand(task.Id, "New title", null, _categoryId, null, TestData.Today.AddDays(-1)),
            CancellationToken);

        result.Error.ShouldBe(TaskErrors.DueDateInPast);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private TaskItem SeedTask(TaskStatus status = TaskStatus.Pending, DateOnly? dueDate = null, DateTimeOffset? createdAt = null)
    {
        var task = TestData.Task(_categoryId, status: status, dueDate: dueDate, createdAt: createdAt);
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        return task;
    }
}
