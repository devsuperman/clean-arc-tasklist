using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Categories;
using TaskList.Domain.Common;

namespace TaskList.Application.Categories.Delete;

internal sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categories,
    ITaskRepository tasks,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound(command.Id);
        }

        // BR-C04: a category with tasks cannot be deleted; the error tells how many tasks are linked.
        var taskCount = await tasks.CountByCategoryAsync(category.Id, cancellationToken);
        if (taskCount > 0)
        {
            return CategoryErrors.HasTasks(taskCount);
        }

        // BR-C05: hard delete.
        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
