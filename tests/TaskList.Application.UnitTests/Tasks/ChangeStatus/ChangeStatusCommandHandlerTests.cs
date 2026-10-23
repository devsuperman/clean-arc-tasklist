using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using TaskList.Application.Abstractions.Data;
using TaskList.Application.Tasks.ChangeStatus;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Common;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.Tasks.ChangeStatus;

public sealed class ChangeStatusCommandHandlerTests
{
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now.AddHours(1));

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public enum StatusAction
    {
        Start,
        Complete,
        Reopen,
    }

    [Theory] // BR-T05, BR-T07
    [InlineData(StatusAction.Start, TaskStatus.Pending, TaskStatus.InProgress)]
    [InlineData(StatusAction.Complete, TaskStatus.Pending, TaskStatus.Done)]
    [InlineData(StatusAction.Complete, TaskStatus.InProgress, TaskStatus.Done)]
    [InlineData(StatusAction.Reopen, TaskStatus.InProgress, TaskStatus.Pending)]
    [InlineData(StatusAction.Reopen, TaskStatus.Done, TaskStatus.Pending)]
    public async Task Handle_AllowedTransition_ChangesStatusAndSaves(StatusAction action, TaskStatus from, TaskStatus expected)
    {
        var task = SeedTask(from);

        var result = await HandleAsync(action, task.Id);

        result.IsSuccess.ShouldBeTrue();
        task.Status.ShouldBe(expected);
        task.UpdatedAt.ShouldBe(_timeProvider.GetUtcNow());
        task.CompletedAt.ShouldBe(expected == TaskStatus.Done ? _timeProvider.GetUtcNow() : null);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory] // BR-T05
    [InlineData(StatusAction.Start, TaskStatus.InProgress)]
    [InlineData(StatusAction.Start, TaskStatus.Done)]
    [InlineData(StatusAction.Complete, TaskStatus.Done)]
    [InlineData(StatusAction.Reopen, TaskStatus.Pending)]
    public async Task Handle_ForbiddenTransition_ReturnsInvalidStatusTransition(StatusAction action, TaskStatus from)
    {
        var task = SeedTask(from);

        var result = await HandleAsync(action, task.Id);

        result.Error.ShouldNotBeNull().Code.ShouldBe("Task.InvalidStatusTransition");
        task.Status.ShouldBe(from);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(StatusAction.Start)]
    [InlineData(StatusAction.Complete)]
    [InlineData(StatusAction.Reopen)]
    public async Task Handle_TaskDoesNotExist_ReturnsTaskNotFound(StatusAction action)
    {
        var id = Guid.NewGuid();

        var result = await HandleAsync(action, id);

        result.Error.ShouldBe(TaskErrors.NotFound(id));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private TaskItem SeedTask(TaskStatus status)
    {
        var task = TestData.Task(Guid.NewGuid(), status: status);
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        return task;
    }

    private Task<Result> HandleAsync(StatusAction action, Guid id) => action switch
    {
        StatusAction.Start => new StartTaskCommandHandler(_tasks, _unitOfWork, _timeProvider)
            .Handle(new StartTaskCommand(id), CancellationToken),
        StatusAction.Complete => new CompleteTaskCommandHandler(_tasks, _unitOfWork, _timeProvider)
            .Handle(new CompleteTaskCommand(id), CancellationToken),
        StatusAction.Reopen => new ReopenTaskCommandHandler(_tasks, _unitOfWork, _timeProvider)
            .Handle(new ReopenTaskCommand(id), CancellationToken),
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };
}
