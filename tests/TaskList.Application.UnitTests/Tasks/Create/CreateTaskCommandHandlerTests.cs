using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using TaskList.Application.Abstractions.Data;
using TaskList.Application.Tasks.Create;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.Tasks.Create;

public sealed class CreateTaskCommandHandlerTests
{
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now);
    private readonly Guid _categoryId = Guid.NewGuid();

    private CreateTaskCommandHandler Handler => new(_tasks, _categories, _unitOfWork, _timeProvider);

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public CreateTaskCommandHandlerTests() =>
        _categories.ExistsAsync(_categoryId, Arg.Any<CancellationToken>()).Returns(true);

    [Fact] // BR-T02, BR-T03
    public async Task Handle_ValidCommand_AddsPendingTaskWithDefaultPriorityAndSaves()
    {
        TaskItem? added = null;
        await _tasks.AddAsync(Arg.Do<TaskItem>(task => added = task), Arg.Any<CancellationToken>());

        var result = await Handler.Handle(
            new CreateTaskCommand("  Read book  ", null, _categoryId, null, TestData.Today),
            CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        added.ShouldNotBeNull();
        added.Id.ShouldBe(result.Value);
        added.Title.ShouldBe("Read book");
        added.Status.ShouldBe(TaskStatus.Pending);
        added.Priority.ShouldBe(TaskPriority.Medium);
        added.CategoryId.ShouldBe(_categoryId);
        added.CreatedAt.ShouldBe(TestData.Now);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-T01
    public async Task Handle_CategoryDoesNotExist_ReturnsCategoryNotFound()
    {
        var unknown = Guid.NewGuid();

        var result = await Handler.Handle(
            new CreateTaskCommand("Read book", null, unknown, TaskPriority.High, null),
            CancellationToken);

        result.Error.ShouldNotBeNull().Code.ShouldBe("Category.NotFound");
        await _tasks.DidNotReceive().AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-T04
    public async Task Handle_DueDateInPast_ReturnsDueDateInPast()
    {
        var result = await Handler.Handle(
            new CreateTaskCommand("Read book", null, _categoryId, null, TestData.Today.AddDays(-1)),
            CancellationToken);

        result.Error.ShouldBe(TaskErrors.DueDateInPast);
        await _tasks.DidNotReceive().AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
