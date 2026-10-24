using NSubstitute;
using TaskList.Application.Abstractions.Data;
using TaskList.Application.Tasks.Delete;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.Tasks.Delete;

public sealed class DeleteTaskCommandHandlerTests
{
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Theory] // BR-T09
    [InlineData(TaskStatus.Pending)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Done)]
    public async Task Handle_ExistingTaskInAnyStatus_RemovesAndSavesOnce(TaskStatus status)
    {
        var task = TestData.Task(Guid.NewGuid(), status: status);
        _tasks.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        var result = await Handler().Handle(new DeleteTaskCommand(task.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _tasks.Received(1).Remove(task);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownTask_ReturnsNotFoundWithoutSaving()
    {
        var id = Guid.NewGuid();

        var result = await Handler().Handle(new DeleteTaskCommand(id), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(TaskErrors.NotFound(id));
        _tasks.DidNotReceive().Remove(Arg.Any<TaskItem>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private DeleteTaskCommandHandler Handler() => new(_tasks, _unitOfWork);
}
