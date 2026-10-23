using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Categories;
using TaskList.Domain.Common;

namespace TaskList.Application.Categories.Update;

internal sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound(command.Id);
        }

        var updated = category.Update(command.Name, command.Description, command.Color, timeProvider.GetUtcNow());
        if (updated.IsFailure)
        {
            return updated;
        }

        // BR-C01, BR-C02: the category itself is excluded, so it can keep its name in another casing.
        if (await categories.NameExistsAsync(category.Name, category.Id, cancellationToken))
        {
            return CategoryErrors.NameAlreadyExists(category.Name);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
