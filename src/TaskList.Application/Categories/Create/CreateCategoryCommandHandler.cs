using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Categories;
using TaskList.Domain.Common;

namespace TaskList.Application.Categories.Create;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var created = Category.Create(command.Name, command.Description, command.Color, timeProvider.GetUtcNow());
        if (created.IsFailure)
        {
            return created.Error;
        }

        var category = created.Value;

        // BR-C01: names are unique ignoring case.
        if (await categories.NameExistsAsync(category.Name, null, cancellationToken))
        {
            return CategoryErrors.NameAlreadyExists(category.Name);
        }

        await categories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
