using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Common;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.Delete;

internal sealed class DeleteTaskCommandHandler(ITaskRepository tasks, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTaskCommand>
{
    public async Task<Result> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(command.Id, cancellationToken);
        if (task is null)
        {
            return TaskErrors.NotFound(command.Id);
        }

        // BR-T09: any status can be deleted.
        tasks.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
