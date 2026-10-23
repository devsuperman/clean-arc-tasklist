using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Categories;
using TaskList.Domain.Common;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.Create;

internal sealed class CreateTaskCommandHandler(
    ITaskRepository taskRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<CreateTaskCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        // BR-T01
        if (!await categoryRepository.ExistsAsync(command.CategoryId, cancellationToken))
        {
            return CategoryErrors.NotFound(command.CategoryId);
        }

        var result = TaskItem.Create(
            command.Title,
            command.Description,
            command.CategoryId,
            command.Priority,
            command.DueDate,
            timeProvider.GetUtcNow());

        if (result.IsFailure)
        {
            return result.Error;
        }

        await taskRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
