using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Common;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.ChangeStatus;

internal sealed class ReopenTaskCommandHandler(
    ITaskRepository taskRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<ReopenTaskCommand>
{
    public async Task<Result> Handle(ReopenTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(command.TaskId, cancellationToken);
        if (task is null)
        {
            return TaskErrors.NotFound(command.TaskId);
        }

        // BR-T05
        var result = task.Reopen(timeProvider.GetUtcNow());
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
