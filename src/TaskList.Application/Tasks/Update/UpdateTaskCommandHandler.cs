using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Categories;
using TaskList.Domain.Common;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.Update;

internal sealed class UpdateTaskCommandHandler(
    ITaskRepository taskRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<UpdateTaskCommand>
{
    public async Task<Result> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(command.TaskId, cancellationToken);
        if (task is null)
        {
            return TaskErrors.NotFound(command.TaskId);
        }

        // BR-T01, BR-T10: moving to another category requires that category to exist.
        if (command.CategoryId != task.CategoryId
            && !await categoryRepository.ExistsAsync(command.CategoryId, cancellationToken))
        {
            return CategoryErrors.NotFound(command.CategoryId);
        }

        var now = timeProvider.GetUtcNow();

        // BR-T04, BR-T08
        var result = task.UpdateDetails(command.Title, command.Description, command.Priority, command.DueDate, now);
        if (result.IsFailure)
        {
            return result;
        }

        result = task.MoveTo(command.CategoryId, now);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
